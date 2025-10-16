using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Exceptions;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Payments;
using AutoPartsStore.Core.Models.Payments.Moyasar;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutoPartsStore.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentTransactionRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMoyasarService _moyasarService;
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentTransactionRepository paymentRepository,
            IOrderRepository orderRepository,
            IMoyasarService moyasarService,
            IOrderService orderService,
            AppDbContext context,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _moyasarService = moyasarService;
            _orderService = orderService;
            _context = context;
            _logger = logger;
        }

        public async Task<MoyasarPaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request)
        {
            _logger.LogInformation("Initiating payment for order {OrderId}", request.OrderId);

            // Get order
            var orderDto = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId);
            if (orderDto == null)
            {
                throw new NotFoundException("«·ÿ·» €Ì— „ÊÃÊœ.", "Order", request.OrderId);
            }

            // Check if order already has a successful payment
            var existingPayment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);
            if (existingPayment != null && 
                (existingPayment.Status == PaymentStatus.Paid || existingPayment.Status == PaymentStatus.Captured))
            {
                throw new BusinessException(" „ œ›⁄ ﬁÌ„… Â–« «·ÿ·» „”»ﬁ«.");
            }

            // Create payment transaction
            var paymentTransaction = new PaymentTransaction(
                request.OrderId,
                orderDto.UserId,
                orderDto.TotalAmount,
                request.PaymentMethod
            );

            await _context.PaymentTransactions.AddAsync(paymentTransaction);
            await _context.SaveChangesAsync();

            // Update order status to PaymentPending
            var order = await _context.Orders.FindAsync(request.OrderId);
            if (order != null)
            {
                order.UpdateStatus(OrderStatus.PaymentPending);
                order.AssignPaymentTransaction(paymentTransaction.Id);
                await _context.SaveChangesAsync();
            }

            // Prepare Moyasar payment request
            var moyasarRequest = new MoyasarCreatePaymentRequest
            {
                Amount = orderDto.TotalAmount,
                Currency = "SAR",
                Description = $"ÿ·» —ﬁ„ {orderDto.OrderNumber}",
                Source = MapPaymentMethodToSource(request),
                CallbackUrl = request.CallbackUrl,
                Metadata = new Dictionary<string, string>
                {
                    ["order_id"] = request.OrderId.ToString(),
                    ["order_number"] = orderDto.OrderNumber,
                    ["transaction_reference"] = paymentTransaction.TransactionReference,
                    ["user_id"] = orderDto.UserId.ToString()
                }
            };

            try
            {
                // Call Moyasar API
                var moyasarResponse = await _moyasarService.CreatePaymentAsync(moyasarRequest);

                // Update payment transaction with Moyasar payment ID
                paymentTransaction.UpdateMoyasarPaymentId(moyasarResponse.Id);
                
                // Store gateway response
                paymentTransaction.UpdateStatus(
                    MapMoyasarStatusToPaymentStatus(moyasarResponse.Status),
                    JsonSerializer.Serialize(moyasarResponse),
                    moyasarResponse.Source?.ReferenceNumber
                );

                // Update card details if available
                if (moyasarResponse.Source?.Number != null)
                {
                    var last4 = moyasarResponse.Source.Number.Length >= 4
                        ? moyasarResponse.Source.Number.Substring(moyasarResponse.Source.Number.Length - 4)
                        : moyasarResponse.Source.Number;
                    
                    paymentTransaction.UpdateCardDetails(last4, moyasarResponse.Source.Company ?? "Unknown");
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Payment initiated successfully. Moyasar PaymentId: {MoyasarPaymentId}, TransactionRef: {TransactionRef}",
                    moyasarResponse.Id, paymentTransaction.TransactionReference);

                return moyasarResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment for order {OrderId}", request.OrderId);

                // Mark payment as failed
                paymentTransaction.MarkAsFailed(ex.Message);
                
                // Update order status
                if (order != null)
                {
                    order.UpdateStatus(OrderStatus.Failed);
                }

                await _context.SaveChangesAsync();

                throw;
            }
        }

        public async Task<PaymentTransactionDto> ProcessPaymentCallbackAsync(string paymentId)
        {
            _logger.LogInformation("Processing payment callback for Moyasar payment {PaymentId}", paymentId);

            // Fetch payment from Moyasar
            var moyasarPayment = await _moyasarService.GetPaymentAsync(paymentId);

            // Find our payment transaction
            var paymentDto = await _paymentRepository.GetByMoyasarPaymentIdAsync(paymentId);
            if (paymentDto == null)
            {
                throw new NotFoundException(
                    "·„ Ì „ «·⁄ÀÊ— ⁄·Ï „⁄«„·… «·œ›⁄.",
                    "PaymentTransaction",
                    paymentId);
            }

            var payment = await _context.PaymentTransactions.FindAsync(paymentDto.Id);
            if (payment == null)
            {
                throw new NotFoundException("„⁄«„·… «·œ›⁄ €Ì— „ÊÃÊœ….");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            if (order == null)
            {
                throw new NotFoundException("«·ÿ·» €Ì— „ÊÃÊœ.");
            }

            var newStatus = MapMoyasarStatusToPaymentStatus(moyasarPayment.Status);

            _logger.LogInformation(
                "Payment callback: Order {OrderNumber}, Status: {Status}",
                order.OrderNumber, moyasarPayment.Status);

            // Update payment transaction
            payment.UpdateStatus(
                newStatus,
                JsonSerializer.Serialize(moyasarPayment),
                moyasarPayment.Source?.ReferenceNumber
            );

            // Handle different payment statuses
            switch (newStatus)
            {
                case PaymentStatus.Paid:
                case PaymentStatus.Captured:
                    await HandleSuccessfulPayment(order, payment);
                    break;

                case PaymentStatus.Failed:
                    await HandleFailedPayment(order, payment, moyasarPayment.Source?.Message);
                    break;

                case PaymentStatus.Authorized:
                    order.UpdateStatus(OrderStatus.PaymentPending);
                    break;
            }

            await _context.SaveChangesAsync();

            return (await _paymentRepository.GetByIdWithDetailsAsync(payment.Id))!;
        }

        public async Task<PaymentTransactionDto> VerifyPaymentAsync(string paymentId)
        {
            _logger.LogInformation("Verifying payment {PaymentId}", paymentId);

            var moyasarPayment = await _moyasarService.GetPaymentAsync(paymentId);
            
            var paymentDto = await _paymentRepository.GetByMoyasarPaymentIdAsync(paymentId);
            if (paymentDto == null)
            {
                throw new NotFoundException("„⁄«„·… «·œ›⁄ €Ì— „ÊÃÊœ….");
            }

            var payment = await _context.PaymentTransactions.FindAsync(paymentDto.Id);
            if (payment == null)
            {
                throw new NotFoundException("„⁄«„·… «·œ›⁄ €Ì— „ÊÃÊœ….");
            }

            var currentStatus = MapMoyasarStatusToPaymentStatus(moyasarPayment.Status);
            
            if (payment.Status != currentStatus)
            {
                _logger.LogInformation(
                    "Payment status changed from {OldStatus} to {NewStatus}",
                    payment.Status, currentStatus);

                payment.UpdateStatus(
                    currentStatus,
                    JsonSerializer.Serialize(moyasarPayment),
                    moyasarPayment.Source?.ReferenceNumber
                );

                await _context.SaveChangesAsync();
            }

            return (await _paymentRepository.GetByIdWithDetailsAsync(payment.Id))!;
        }

        public async Task<PaymentTransactionDto?> GetPaymentByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<PaymentTransactionDto?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _paymentRepository.GetByOrderIdAsync(orderId);
        }

        public async Task<PagedResult<PaymentTransactionDto>> GetPaymentsAsync(PaymentFilterRequest filter)
        {
            return await _paymentRepository.GetFilteredTransactionsAsync(filter);
        }

        public async Task<List<PaymentTransactionDto>> GetUserPaymentsAsync(int userId)
        {
            return await _paymentRepository.GetUserTransactionsAsync(userId);
        }

        public async Task<PaymentTransactionDto> RefundPaymentAsync(int paymentId, RefundPaymentRequest request)
        {
            _logger.LogInformation("Processing refund for payment {PaymentId}, Amount: {Amount}",
                paymentId, request.Amount);

            var paymentDto = await _paymentRepository.GetByIdWithDetailsAsync(paymentId);
            if (paymentDto == null)
            {
                throw new NotFoundException("„⁄«„·… «·œ›⁄ €Ì— „ÊÃÊœ….", "PaymentTransaction", paymentId);
            }

            var payment = await _context.PaymentTransactions.FindAsync(paymentId);
            if (payment == null)
            {
                throw new NotFoundException("„⁄«„·… «·œ›⁄ €Ì— „ÊÃÊœ….");
            }

            if (!payment.CanBeRefunded())
            {
                throw new BusinessException("·« Ì„ﬂ‰ «” —œ«œ ﬁÌ„… Â–Â «·„⁄«„·….");
            }

            if (string.IsNullOrEmpty(payment.MoyasarPaymentId))
            {
                throw new BusinessException("„⁄—¯› «·œ›⁄ ›Ì Moyasar €Ì— „ÊÃÊœ.");
            }

            // Call Moyasar refund API
            var refundRequest = new MoyasarRefundRequest
            {
                Amount = (int)(request.Amount * 100) // Convert to halalas
            };

            var moyasarResponse = await _moyasarService.RefundPaymentAsync(
                payment.MoyasarPaymentId,
                refundRequest);

            // Update payment transaction
            payment.ProcessRefund(
                request.Amount,
                request.Reason,
                moyasarResponse.Id);

            // Update order status
            var order = await _context.Orders.FindAsync(payment.OrderId);
            if (order != null)
            {
                order.UpdateStatus(OrderStatus.Refunded);
                
                // Restore stock
                await _orderService.ReduceStockAsync(order); // This actually adds stock back
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refund processed successfully for payment {PaymentId}", paymentId);

            return (await _paymentRepository.GetByIdWithDetailsAsync(paymentId))!;
        }

        public async Task<PaymentSummaryDto> GetPaymentSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _paymentRepository.GetPaymentSummaryAsync(fromDate, toDate);
        }

        // Private helper methods
        private async Task HandleSuccessfulPayment(Order order, PaymentTransaction payment)
        {
            _logger.LogInformation("Handling successful payment for order {OrderNumber}", order.OrderNumber);

            // Update order status
            order.UpdateStatus(OrderStatus.Paid);

            // Reduce stock
            await _orderService.ReduceStockAsync(order);

            _logger.LogInformation("Order {OrderNumber} marked as paid and stock reduced", order.OrderNumber);
        }

        private async Task HandleFailedPayment(Order order, PaymentTransaction payment, string? errorMessage)
        {
            _logger.LogWarning("Payment failed for order {OrderNumber}. Reason: {Reason}",
                order.OrderNumber, errorMessage);

            payment.MarkAsFailed(errorMessage ?? "Payment failed", "MOYASAR_PAYMENT_FAILED");
            order.UpdateStatus(OrderStatus.Failed);

            await Task.CompletedTask;
        }

        private MoyasarSource MapPaymentMethodToSource(InitiatePaymentRequest request)
        {
            var source = new MoyasarSource();

            switch (request.PaymentMethod)
            {
                case PaymentMethod.CreditCard:
                case PaymentMethod.Mada:
                    source.Type = MoyasarSourceType.CreditCard;
                    source.Number = request.CardNumber;
                    source.Name = request.CardHolderName;
                    source.Month = request.ExpiryMonth;
                    source.Year = request.ExpiryYear;
                    source.Cvc = request.CVV;
                    break;

                case PaymentMethod.ApplePay:
                    source.Type = MoyasarSourceType.ApplePay;
                    // Token would be provided by Apple Pay SDK
                    break;

                case PaymentMethod.STCPay:
                    source.Type = MoyasarSourceType.STCPay;
                    break;

                case PaymentMethod.Tabby:
                    source.Type = MoyasarSourceType.Tabby;
                    source.Company = "tabby";
                    break;

                case PaymentMethod.Tamara:
                    source.Type = MoyasarSourceType.Tamara;
                    source.Company = "tamara";
                    break;

                default:
                    throw new ValidationException("ÿ—Ìﬁ… «·œ›⁄ €Ì— „œ⁄Ê„….");
            }

            return source;
        }

        private PaymentStatus MapMoyasarStatusToPaymentStatus(string moyasarStatus)
        {
            return moyasarStatus.ToLower() switch
            {
                MoyasarPaymentStatus.Initiated => PaymentStatus.Initiated,
                MoyasarPaymentStatus.Paid => PaymentStatus.Paid,
                MoyasarPaymentStatus.Failed => PaymentStatus.Failed,
                MoyasarPaymentStatus.Authorized => PaymentStatus.Authorized,
                MoyasarPaymentStatus.Captured => PaymentStatus.Captured,
                MoyasarPaymentStatus.Refunded => PaymentStatus.Refunded,
                MoyasarPaymentStatus.Voided => PaymentStatus.Voided,
                _ => PaymentStatus.Pending
            };
        }
    }
}

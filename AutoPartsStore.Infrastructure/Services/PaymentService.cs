using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Exceptions;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Orders;  // Add this using for OrderDto
using AutoPartsStore.Core.Models.Payments;
using AutoPartsStore.Core.Models.Payments.Tap;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AutoPartsStore.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentTransactionRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ITapService _tapService;
        private readonly TapSettings _tapSettings;
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentTransactionRepository paymentRepository,
            IOrderRepository orderRepository,
            ITapService tapService,
            IOptions<TapSettings> tapSettings,
            IOrderService orderService,
            AppDbContext context,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _tapService = tapService;
            _tapSettings = tapSettings.Value;
            _orderService = orderService;
            _context = context;
            _logger = logger;
        }

        public async Task<TapChargeResponse> InitiatePaymentAsync(InitiatePaymentRequest request)
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
                throw new BusinessException(" „ «·œ›⁄ ·Â–« «·ÿ·» »«·›⁄·.");
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

            // Prepare Tap charge request
            var tapRequest = BuildTapChargeRequest(request, orderDto, paymentTransaction);

            try
            {
                // Call Tap API
                var tapResponse = await _tapService.CreateChargeAsync(tapRequest);

                // Update payment transaction with Tap charge ID
                paymentTransaction.UpdateTapChargeId(tapResponse.Id);
                
                // Store gateway response
                paymentTransaction.UpdateStatus(
                    MapTapStatusToPaymentStatus(tapResponse.Status),
                    JsonSerializer.Serialize(tapResponse),
                    tapResponse.Reference?.Transaction
                );

                // Update card details if available
                if (tapResponse.Card != null)
                {
                    paymentTransaction.UpdateCardDetails(
                        tapResponse.Card.LastFour ?? "",
                        tapResponse.Card.Brand ?? "",
                        tapResponse.Card.Scheme
                    );
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Payment initiated successfully. Tap ChargeId: {ChargeId}, TransactionRef: {TransactionRef}",
                    tapResponse.Id, paymentTransaction.TransactionReference);

                return tapResponse;
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

        public async Task<PaymentTransactionDto> ProcessPaymentWebhookAsync(string chargeId)
        {
            _logger.LogInformation("Processing payment webhook for Tap charge {ChargeId}", chargeId);

            // Fetch payment from Tap
            var tapCharge = await _tapService.GetChargeAsync(chargeId);

            // Find our payment transaction
            var paymentDto = await _paymentRepository.GetByTapChargeIdAsync(chargeId);
            if (paymentDto == null)
            {
                throw new NotFoundException(
                    "·„ Ì „ «·⁄ÀÊ— ⁄·Ï „⁄«„·… «·œ›⁄.",
                    "PaymentTransaction",
                    chargeId);
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

            var newStatus = MapTapStatusToPaymentStatus(tapCharge.Status);

            _logger.LogInformation(
                "Payment webhook: Order {OrderNumber}, Status: {Status}",
                order.OrderNumber, tapCharge.Status);

            // Update payment transaction
            payment.UpdateStatus(
                newStatus,
                JsonSerializer.Serialize(tapCharge),
                tapCharge.Reference?.Transaction
            );

            // Handle different payment statuses
            switch (newStatus)
            {
                case PaymentStatus.Paid:
                case PaymentStatus.Captured:
                    await HandleSuccessfulPayment(order, payment);
                    break;

                case PaymentStatus.Failed:
                case PaymentStatus.Declined:
                    await HandleFailedPayment(order, payment, tapCharge.Response?.Message);
                    break;

                case PaymentStatus.Authorized:
                    order.UpdateStatus(OrderStatus.PaymentPending);
                    break;
            }

            await _context.SaveChangesAsync();

            return (await _paymentRepository.GetByIdWithDetailsAsync(payment.Id))!;
        }

        public async Task<PaymentTransactionDto> VerifyPaymentAsync(string chargeId)
        {
            _logger.LogInformation("Verifying payment {ChargeId}", chargeId);

            var tapCharge = await _tapService.GetChargeAsync(chargeId);
            
            var paymentDto = await _paymentRepository.GetByTapChargeIdAsync(chargeId);
            if (paymentDto == null)
            {
                throw new NotFoundException("„⁄«„·… «·œ›⁄ €Ì— „ÊÃÊœ….");
            }

            var payment = await _context.PaymentTransactions.FindAsync(paymentDto.Id);
            if (payment == null)
            {
                throw new NotFoundException("„⁄«„·… «·œ›⁄ €Ì— „ÊÃÊœ….");
            }

            var currentStatus = MapTapStatusToPaymentStatus(tapCharge.Status);
            
            if (payment.Status != currentStatus)
            {
                _logger.LogInformation(
                    "Payment status changed from {OldStatus} to {NewStatus}",
                    payment.Status, currentStatus);

                payment.UpdateStatus(
                    currentStatus,
                    JsonSerializer.Serialize(tapCharge),
                    tapCharge.Reference?.Transaction
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
                throw new BusinessException("·« Ì„ﬂ‰ «” —œ«œ Â–Â «·œ›⁄….");
            }

            if (string.IsNullOrEmpty(payment.TapChargeId))
            {
                throw new BusinessException("„⁄—› «·œ›⁄… ›Ì Tap €Ì— „ÊÃÊœ.");
            }

            // Call Tap refund API
            var refundRequest = new TapRefundRequest
            {
                ChargeId = payment.TapChargeId,
                Amount = request.Amount,
                Currency = "SAR",
                Reason = request.Reason,
                Description = $"Refund for order {paymentDto.OrderNumber}"
            };

            var tapResponse = await _tapService.RefundChargeAsync(refundRequest);

            // Update payment transaction
            payment.ProcessRefund(
                request.Amount,
                request.Reason,
                tapResponse.Id);

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

            payment.MarkAsFailed(errorMessage ?? "Payment failed", "TAP_PAYMENT_FAILED");
            order.UpdateStatus(OrderStatus.Failed);

            await Task.CompletedTask;
        }

        private TapCreateChargeRequest BuildTapChargeRequest(
            InitiatePaymentRequest request,
            OrderDto orderDto,
            PaymentTransaction paymentTransaction)
        {
            // Split name for Tap
            var nameParts = SplitName(request.FirstName + " " + request.LastName);

            var tapRequest = new TapCreateChargeRequest
            {
                Amount = orderDto.TotalAmount,
                Currency = "SAR",
                ThreeDSecure = _tapSettings.Enable3DSecure,
                SaveCard = _tapSettings.SaveCards,
                Description = $"ÿ·» —ﬁ„ {orderDto.OrderNumber}",
                StatementDescriptor = _tapSettings.StatementDescriptor,
                Metadata = new TapMetadata
                {
                    OrderId = request.OrderId.ToString(),
                    OrderNumber = orderDto.OrderNumber,
                    UserId = orderDto.UserId.ToString(),
                    TransactionReference = paymentTransaction.TransactionReference
                },
                Reference = new TapReference
                {
                    Transaction = paymentTransaction.TransactionReference,
                    Order = orderDto.OrderNumber
                },
                Customer = new TapCustomer
                {
                    FirstName = nameParts.firstName,
                    LastName = nameParts.lastName,
                    Email = request.Email,
                    Phone = new TapPhone
                    {
                        CountryCode = "966",
                        Number = CleanPhoneNumber(request.PhoneNumber)
                    }
                },
                Source = MapPaymentMethodToSource(request),
                Redirect = new TapRedirect
                {
                    Url = request.RedirectUrl ?? _tapSettings.RedirectUrl
                },
                Post = new TapPost
                {
                    Url = request.WebhookUrl ?? _tapSettings.WebhookUrl
                }
            };

            return tapRequest;
        }

        private TapSource MapPaymentMethodToSource(InitiatePaymentRequest request)
        {
            var source = new TapSource();

            switch (request.PaymentMethod)
            {
                case PaymentMethod.Visa:
                case PaymentMethod.MasterCard:
                case PaymentMethod.Mada:
                    // For cards, use the token from Tap.js
                    if (string.IsNullOrEmpty(request.TapToken))
                    {
                        throw new ValidationException("Tap token is required for card payments.");
                    }
                    source.Id = request.TapToken;  // Token from Tap.js (tok_xxxx)
                    break;

                case PaymentMethod.ApplePay:
                    // For Apple Pay, use the token from Apple Pay SDK
                    if (string.IsNullOrEmpty(request.ApplePayToken))
                    {
                        throw new ValidationException("Apple Pay token is required for Apple Pay payments.");
                    }
                    source.Id = request.ApplePayToken;
                    break;

                case PaymentMethod.Tabby:
                    // For Tabby, use the specific source ID
                    source.Id = TapSourceType.Tabby;
                    break;

                default:
                    throw new ValidationException("ÿ—Ìﬁ… «·œ›⁄ €Ì— „œ⁄Ê„….");
            }

            return source;
        }

        private PaymentStatus MapTapStatusToPaymentStatus(string tapStatus)
        {
            return tapStatus?.ToUpper() switch
            {
                TapPaymentStatus.Initiated => PaymentStatus.Initiated,
                TapPaymentStatus.InProgress => PaymentStatus.Pending,
                TapPaymentStatus.Captured => PaymentStatus.Captured,
                TapPaymentStatus.Failed => PaymentStatus.Failed,
                TapPaymentStatus.Declined => PaymentStatus.Declined,
                TapPaymentStatus.Cancelled => PaymentStatus.Cancelled,
                TapPaymentStatus.Abandoned => PaymentStatus.Abandoned,
                TapPaymentStatus.Void => PaymentStatus.Voided,
                _ => PaymentStatus.Pending
            };
        }

        private (string firstName, string lastName) SplitName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return ("Customer", "");

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
                return ("Customer", "");
            if (parts.Length == 1)
                return (parts[0], "");
            
            return (parts[0], string.Join(" ", parts.Skip(1)));
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Remove leading zeros or country code
            if (cleaned.StartsWith("966"))
                cleaned = cleaned.Substring(3);
            else if (cleaned.StartsWith("0"))
                cleaned = cleaned.Substring(1);
            
            return cleaned;
        }
    }
}

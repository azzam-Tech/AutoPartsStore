using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Initiate payment for an order
        /// </summary>
        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
        {
            _logger.LogInformation("Initiating payment for order {OrderId}", request.OrderId);

            var moyasarResponse = await _paymentService.InitiatePaymentAsync(request);

            return Success(new
            {
                paymentId = moyasarResponse.Id,
                status = moyasarResponse.Status,
                amount = moyasarResponse.Amount / 100.0m, // Convert from halalas to SAR
                currency = moyasarResponse.Currency,
                transactionUrl = moyasarResponse.Source?.TransactionUrl
            }, " „ »œ¡ ⁄„·Ì… «·œ›⁄ »‰Ã«Õ.");
        }

        /// <summary>
        /// Moyasar payment callback webhook
        /// This endpoint receives notifications from Moyasar when payment status changes
        /// </summary>
        [HttpPost("callback")]
        [AllowAnonymous] // Moyasar needs to access this without authentication
        public async Task<IActionResult> PaymentCallback([FromBody] ProcessPaymentCallbackRequest request)
        {
            _logger.LogInformation("Received payment callback for payment {PaymentId}", request.PaymentId);

            try
            {
                var payment = await _paymentService.ProcessPaymentCallbackAsync(request.PaymentId);
                
                return Success(payment, " „ „⁄«·Ã… ‰ ÌÃ… «·œ›⁄ »‰Ã«Õ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback for {PaymentId}", request.PaymentId);
                
                // Return 200 OK even on error to prevent Moyasar from retrying
                return Ok(new
                {
                    success = false,
                    message = " „ «” ·«„ «·ÿ·» Ê·ﬂ‰ ÕœÀ Œÿ√ ›Ì «·„⁄«·Ã….",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Verify payment status with Moyasar
        /// </summary>
        [HttpPost("verify/{paymentId}")]
        [Authorize]
        public async Task<IActionResult> VerifyPayment(string paymentId)
        {
            var payment = await _paymentService.VerifyPaymentAsync(paymentId);
            return Success(payment, " „ «· Õﬁﬁ „‰ Õ«·… «·œ›⁄.");
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound("„⁄«„·… «·œ›⁄ €Ì— „ÊÃÊœ….");

            var userId = GetAuthenticatedUserId();
            var isAdmin = User.IsInRole("Admin");

            // Users can only see their own payments
            if (payment.UserId != userId && !isAdmin)
                return Forbid();

            return Success(payment);
        }

        /// <summary>
        /// Get payment by order ID
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentByOrder(int orderId)
        {
            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (payment == null)
                return NotFound("·„ Ì „ «·⁄ÀÊ— ⁄·Ï „⁄«„·… œ›⁄ ·Â–« «·ÿ·».");

            var userId = GetAuthenticatedUserId();
            var isAdmin = User.IsInRole("Admin");

            if (payment.UserId != userId && !isAdmin)
                return Forbid();

            return Success(payment);
        }

        /// <summary>
        /// Get all payments (Admin only) with filtering
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPayments([FromQuery] PaymentFilterRequest filter)
        {
            var payments = await _paymentService.GetPaymentsAsync(filter);
            return Success(payments);
        }

        /// <summary>
        /// Get user's payments
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserPayments(int userId)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            var isAdmin = User.IsInRole("Admin");

            // Users can only see their own payments
            if (userId != authenticatedUserId && !isAdmin)
                return Forbid();

            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            return Success(payments);
        }

        /// <summary>
        /// Get authenticated user's payments
        /// </summary>
        [HttpGet("my-payments")]
        [Authorize]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = GetAuthenticatedUserId();
            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            return Success(payments);
        }

        /// <summary>
        /// Process payment refund (Admin only)
        /// </summary>
        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RefundPayment(int id, [FromBody] RefundPaymentRequest request)
        {
            _logger.LogInformation("Processing refund for payment {PaymentId}, Amount: {Amount}",
                id, request.Amount);

            var payment = await _paymentService.RefundPaymentAsync(id, request);
            return Success(payment, " „ «” —œ«œ «·„»·€ »‰Ã«Õ.");
        }

        /// <summary>
        /// Get payment statistics (Admin only)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var summary = await _paymentService.GetPaymentSummaryAsync(fromDate, toDate);
            return Success(summary);
        }

        /// <summary>
        /// Get payment summary for specific period (Admin only)
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentSummary(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var summary = await _paymentService.GetPaymentSummaryAsync(fromDate, toDate);

            var result = new
            {
                summary.TotalTransactions,
                summary.TotalAmount,
                summary.SuccessfulPayments,
                summary.FailedPayments,
                summary.RefundedPayments,
                summary.TotalRefunded,
                successRate = summary.TotalTransactions > 0
                    ? (double)summary.SuccessfulPayments / summary.TotalTransactions * 100
                    : 0,
                averageTransactionAmount = summary.SuccessfulPayments > 0
                    ? summary.TotalAmount / summary.SuccessfulPayments
                    : 0
            };

            return Success(result);
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("„⁄—¯› «·„” Œœ„ €Ì— „ÊÃÊœ.");
            return int.Parse(userIdClaim);
        }
    }
}

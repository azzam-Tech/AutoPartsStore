using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Payments;
using AutoPartsStore.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using AutoPartsStore.Core.Models.Payments.Tap;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly TapWebhookValidator _webhookValidator;
        private readonly TapSettings _tapSettings;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger,
            TapWebhookValidator webhookValidator,
            IOptions<TapSettings> tapSettings)
        {
            _paymentService = paymentService;
            _logger = logger;
            _webhookValidator = webhookValidator;
            _tapSettings = tapSettings.Value;
        }

        /// <summary>
        /// Initiate payment for an order using Tap Payment Gateway
        /// </summary>
        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
        {
            _logger.LogInformation("Initiating payment for order {OrderId}", request.OrderId);

            var tapResponse = await _paymentService.InitiatePaymentAsync(request);

            return Success(new
            {
                chargeId = tapResponse.Id,
                status = tapResponse.Status,
                amount = tapResponse.Amount,
                currency = tapResponse.Currency,
                redirectUrl = tapResponse.Transaction?.Url,  // URL for 3D Secure
                transactionUrl = tapResponse.Transaction?.Url
            }, " „ »œ¡ ⁄„·Ì… «·œ›⁄ »‰Ã«Õ.");
        }

        /// <summary>
        /// ? WEBHOOK: Tap payment webhook endpoint (Steps 8-9)
        /// This endpoint receives notifications from Tap when payment status changes
        /// IMPORTANT: Validates webhook signature (hashstring) for security
        /// Reference: https://developers.tap.company/docs/webhook
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous] // ? Tap needs to access this without authentication
        public async Task<IActionResult> TapWebhook()
        {
            try
            {
                // Step 1: Get hashstring from header
                var receivedHash = Request.Headers["hashstring"].FirstOrDefault();
                if (string.IsNullOrEmpty(receivedHash))
                {
                    _logger.LogWarning("Webhook received without hashstring header");
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Missing hashstring header"
                    });
                }

                // Step 2: Read raw JSON body
                string jsonPayload;
                using (var reader = new StreamReader(Request.Body))
                {
                    jsonPayload = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrEmpty(jsonPayload))
                {
                    _logger.LogWarning("Webhook received with empty body");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Empty payload"
                    });
                }

                // Step 3: Validate signature (HMAC-SHA256)
                var isValid = _webhookValidator.ValidateFromJson(
                    jsonPayload,
                    receivedHash,
                    _tapSettings.SecretKey);

                if (!isValid)
                {
                    _logger.LogWarning("Webhook signature validation failed. Hash: {Hash}", receivedHash);
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid signature"
                    });
                }

                // Step 4: Parse payload
                var payload = System.Text.Json.JsonSerializer.Deserialize<TapWebhookPayload>(jsonPayload);
                if (payload == null)
                {
                    _logger.LogError("Failed to deserialize webhook payload");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid payload format"
                    });
                }

                _logger.LogInformation(
                    "? Webhook signature validated. ChargeId: {ChargeId}, Status: {Status}",
                    payload.Id, payload.Status);

                // Step 5: Process the webhook
                var payment = await _paymentService.ProcessTapWebhookAsync(payload);

                _logger.LogInformation(
                    "Webhook processed successfully. Order: {OrderNumber}, Status: {Status}",
                    payment.OrderNumber, payment.Status);

                // ? IMPORTANT: Always return 200 OK to Tap (even on success)
                // This prevents Tap from retrying the webhook
                return Ok(new
                {
                    success = true,
                    message = " „ «” ·«„ «·≈‘⁄«— Ê„⁄«·Ã Â »‰Ã«Õ",
                    chargeId = payload.Id,
                    orderId = payment.OrderId,
                    orderNumber = payment.OrderNumber,
                    status = payment.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Tap webhook");

                // ? IMPORTANT: Return 200 OK even on error
                // Log the error but don't let Tap retry
                return Ok(new
                {
                    success = false,
                    message = " „ «” ·«„ «·≈‘⁄«— ·ﬂ‰ ÕœÀ Œÿ√ ›Ì «·„⁄«·Ã…",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Verify payment status with Tap
        /// </summary>
        [HttpPost("verify/{chargeId}")]
        [Authorize]
        public async Task<IActionResult> VerifyPayment(string chargeId)
        {
            var payment = await _paymentService.VerifyPaymentAsync(chargeId);
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
                return NotFound("·„ Ì „ «·⁄ÀÊ— ⁄·Ï ⁄„·Ì… œ›⁄ ·Â–« «·ÿ·».");

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
            return Success(payment, " „ «” —œ«œ «·œ›⁄… »‰Ã«Õ.");
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
                throw new UnauthorizedAccessException("„⁄—› «·„” Œœ„ €Ì— „ÊÃÊœ.");
            return int.Parse(userIdClaim);
        }
    }
}

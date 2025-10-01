using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Feedbacks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/feedback")]
    public class CustomerFeedbackController : BaseController
    {
        private readonly ICustomerFeedbackService _feedbackService;
        private readonly ILogger<CustomerFeedbackController> _logger;

        public CustomerFeedbackController(
            ICustomerFeedbackService feedbackService,
            ILogger<CustomerFeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFeedbacks([FromQuery] FeedbackFilterRequest filter)
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync(filter);
            return Success(feedbacks);
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserFeedbacks(int userId)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            if (authenticatedUserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var feedbacks = await _feedbackService.GetUserFeedbacksAsync(userId);
            return Success(feedbacks);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);

            if (feedback == null)
                return NotFound("Feedback not found");

            // التحقق من الصلاحيات
            var authenticatedUserId = GetAuthenticatedUserId();
            if (feedback.UserId != authenticatedUserId && !User.IsInRole("Admin"))
                return Forbid();

            return Success(feedback);
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFeedbackStats()
        {
            var stats = await _feedbackService.GetFeedbackStatsAsync();
            return  Success(stats);
        }

        [HttpGet("recent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRecentFeedbacks([FromQuery] int count = 10)
        {
            var feedbacks = await _feedbackService.GetRecentFeedbacksAsync(count);
            return Success(feedbacks);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequest request)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                var feedback = await _feedbackService.CreateFeedbackAsync(userId, request);
                return Success(feedback, "Feedback submitted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] UpdateFeedbackRequest request)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                var feedback = await _feedbackService.UpdateFeedbackAsync(id, userId, request);
                return Success(feedback, "Feedback updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                await _feedbackService.DeleteFeedbackAsync(id, userId);
                return Success("Feedback deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my-feedbacks")]
        [Authorize]
        public async Task<IActionResult> GetMyFeedbacks()
        {
            var userId = GetAuthenticatedUserId();
            var feedbacks = await _feedbackService.GetUserFeedbacksAsync(userId);
            return Success(feedbacks);
        }

        [HttpPost("my-feedbacks")]
        [Authorize]
        public async Task<IActionResult> CreateMyFeedback([FromBody] CreateFeedbackRequest request)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                var feedback = await _feedbackService.CreateFeedbackAsync(userId, request);
                return Success(feedback, "Your feedback has been submitted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("featured")]
        [AllowAnonymous] // متاح للجميع
        public async Task<IActionResult> GetFeaturedFeedbacks()
        {
            var featured = await _feedbackService.GetFeaturedFeedbacksAsync();
            return Success(featured);
        }

        [HttpPatch("{id}/feature")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleFeaturedStatus(int id, [FromBody] FeatureFeedbackRequest request)
        {
            try
            {
                var result = await _feedbackService.ToggleFeaturedStatusAsync(id, request.IsFeatured);

                if (!result)
                    return NotFound("Feedback not found");

                var message = request.IsFeatured ? "Feedback featured successfully" : "Feedback unfeatured successfully";
                return Success(message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new InvalidOperationException("Authenticated user ID claim is missing.");
            return int.Parse(userIdClaim);
        }
    }
}

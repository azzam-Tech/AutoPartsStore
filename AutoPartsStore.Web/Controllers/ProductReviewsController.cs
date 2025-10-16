using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ProductReviewsController : BaseController
    {
        private readonly IProductReviewService _reviewService;
        private readonly ILogger<ProductReviewsController> _logger;

        public ProductReviewsController(
            IProductReviewService reviewService,
            ILogger<ProductReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReviews([FromQuery] ProductReviewstatus? productReviewstatus)
        {
            var reviews = await _reviewService.GetReviewsAsync(productReviewstatus);
            return Success(reviews);
        }

        [HttpGet("product/{partId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductReviews(int partId, [FromQuery] ProductReviewstatus? productReviewstatus)
        {
            var reviews = await _reviewService.GetProductReviewsAsync(partId, productReviewstatus);
            return Success(reviews);
        }

        [HttpGet("product/{partId}/summary")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewSummary(int partId)
        {
            var summary = await _reviewService.GetReviewSummaryAsync(partId);
            return Success(summary);
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserReviews(int userId)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            if (authenticatedUserId != userId)
                return Forbid();

            var reviews = await _reviewService.GetUserReviewsAsync(userId);
            return Success(reviews);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingReviews()
        {
            var reviews = await _reviewService.GetPendingReviewsAsync();
            return Success(reviews);
        }

        [HttpGet("{reviewId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReview(int reviewId)
        {
            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            return review != null ? Success(review) : NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                var review = await _reviewService.CreateReviewAsync(userId, request);
                return Success(review, "Review submitted successfully. Waiting for approval.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewRequest request)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                var review = await _reviewService.UpdateReviewAsync(reviewId, userId, request);
                return Success(review, "Review updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                await _reviewService.DeleteReviewAsync(reviewId, userId);
                return Success("Review deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Approve, reject, or set review to pending
        /// </summary>
        /// <param name="reviewId">Review ID</param>
        /// <param name="request">Approval request (null = Pending, true = Approved, false = Rejected)</param>
        [HttpPatch("{reviewId}/approval")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveReview(int reviewId, [FromBody] ReviewApprovalRequest request)
        {
            try
            {
                await _reviewService.ApproveReviewAsync(reviewId, request.IsApproved);
                
                var statusMessage = request.IsApproved switch
                {
                    true => "approved",
                    false => "rejected",
                    null => "set to pending"
                };
                
                return Success($"Review {statusMessage} successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim);
        }
    }
}
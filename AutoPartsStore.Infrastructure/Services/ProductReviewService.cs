using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Review;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class ProductReviewService : IProductReviewService
    {
        private readonly IProductReviewRepository _reviewRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<ProductReviewService> _logger;

        public ProductReviewService(
            IProductReviewRepository reviewRepository,
            AppDbContext context,
            ILogger<ProductReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<List<ProductReviewDto>> GetReviewsAsync(ProductReviewstatus? productReviewstatus)
        {
            return await _reviewRepository.GetReviewsAsync(productReviewstatus);
        }

        public async Task<List<ProductReviewDto>> GetProductReviewsAsync(int partId, ProductReviewstatus? productReviewstatus)
        {
            return await _reviewRepository.GetReviewsByPartIdAsync(partId, productReviewstatus);
        }

        public async Task<List<ProductReviewDto>> GetUserReviewsAsync(int userId)
        {
            return await _reviewRepository.GetReviewsByUserIdAsync(userId);
        }

        public async Task<List<ProductReviewDto>> GetPendingReviewsAsync()
        {
            return await _reviewRepository.GetPendingReviewsAsync();
        }

        public async Task<ProductReviewDto> GetReviewByIdAsync(int reviewId)
        {
            return await _reviewRepository.GetReviewWithDetailsAsync(reviewId);
        }

        public async Task<ProductReviewDto> CreateReviewAsync(int userId, CreateReviewRequest request)
        {
            // التحقق من وجود المنتج
            var part = await _context.CarParts
                .FirstOrDefaultAsync(p => p.Id == request.PartId && p.IsActive && !p.IsDeleted);

            if (part == null)
                throw new KeyNotFoundException("Product not found or unavailable");

            // التحقق إذا كان المستخدم قد قام بتقييم هذا المنتج من قبل
            if (await _reviewRepository.HasUserReviewedPartAsync(userId, request.PartId))
                throw new InvalidOperationException("You have already reviewed this product");

            // إنشاء التقييم
            var review = new ProductReview(
                request.PartId,
                userId,
                request.Rating,
                request.ReviewText
            );

            await _context.ProductReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Review created for product {PartId} by user {UserId}", request.PartId, userId);
            return await _reviewRepository.GetReviewWithDetailsAsync(review.Id);
        }

        public async Task<ProductReviewDto> UpdateReviewAsync(int reviewId, int userId, UpdateReviewRequest request)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
                throw new KeyNotFoundException("Review not found or you don't have permission to edit it");

            if (request.Rating.HasValue)
            {
                if (request.Rating.Value < 1 || request.Rating.Value > 5)
                    throw new ArgumentException("Rating must be between 1 and 5");

                // استخدام Reflection لتعديل الخاصية الخاصة
                var ratingProperty = review.GetType().GetProperty("Rating");
                ratingProperty?.SetValue(review, request.Rating.Value);
            }

            if (request.ReviewText != null)
            {
                var reviewTextProperty = review.GetType().GetProperty("ReviewText");
                reviewTextProperty?.SetValue(review, request.ReviewText);
            }

            _context.ProductReviews.Update(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Review {ReviewId} updated by user {UserId}", reviewId, userId);
            return await _reviewRepository.GetReviewWithDetailsAsync(reviewId);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
                throw new KeyNotFoundException("Review not found or you don't have permission to delete it");

            _context.ProductReviews.Remove(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Review {ReviewId} deleted by user {UserId}", reviewId, userId);
            return true;
        }

        /// <summary>
        /// Approve, reject, or set review to pending
        /// </summary>
        /// <param name="reviewId">Review ID</param>
        /// <param name="isApproved">null = Pending, true = Approved, false = Rejected</param>
        public async Task<bool> ApproveReviewAsync(int reviewId, bool? isApproved)
        {
            var review = await _context.ProductReviews.FindAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException("Review not found");

            if (isApproved == true)
            {
                review.Approve();
                _logger.LogInformation("Review {ReviewId} approved", reviewId);
            }
            else if (isApproved == false)
            {
                review.Reject();
                _logger.LogInformation("Review {ReviewId} rejected", reviewId);
            }
            else
            {
                review.SetPending();
                _logger.LogInformation("Review {ReviewId} set to pending", reviewId);
            }

            _context.ProductReviews.Update(review);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ReviewSummaryDto> GetReviewSummaryAsync(int partId)
        {
            return await _reviewRepository.GetReviewSummaryAsync(partId);
        }
    }
}
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

        public async Task<List<ProductReviewDto>> GetProductReviewsAsync(int partId, bool? approvedOnly = true)
        {
            return await _reviewRepository.GetReviewsByPartIdAsync(partId, approvedOnly);
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

        public async Task<bool> ApproveReviewAsync(int reviewId, bool isApproved)
        {
            var review = await _context.ProductReviews.FindAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException("Review not found");

            if (isApproved)
                review.Approve();
            else
                review.Reject();

            _context.ProductReviews.Update(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Review {ReviewId} {Status}", reviewId, isApproved ? "approved" : "rejected");
            return true;
        }

        public async Task<ReviewSummaryDto> GetReviewSummaryAsync(int partId)
        {
            return await _reviewRepository.GetReviewSummaryAsync(partId);
        }

    }
}
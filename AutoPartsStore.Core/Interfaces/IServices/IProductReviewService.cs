using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Review;

public interface IProductReviewService
{
    Task<List<ProductReviewDto>> GetProductReviewsAsync(int partId, ProductReviewstatus? productReviewstatus);
    Task<List<ProductReviewDto>> GetUserReviewsAsync(int userId);
    Task<List<ProductReviewDto>> GetPendingReviewsAsync();
    Task<ProductReviewDto> GetReviewByIdAsync(int reviewId);
    Task<ProductReviewDto> CreateReviewAsync(int userId, CreateReviewRequest request);
    Task<ProductReviewDto> UpdateReviewAsync(int reviewId, int userId, UpdateReviewRequest request);
    Task<bool> DeleteReviewAsync(int reviewId, int userId);
    Task<bool> ApproveReviewAsync(int reviewId, bool? isApproved); // Updated to nullable bool
    Task<ReviewSummaryDto> GetReviewSummaryAsync(int partId);
    Task<List<ProductReviewDto>> GetReviewsAsync(ProductReviewstatus? productReviewstatus);
}
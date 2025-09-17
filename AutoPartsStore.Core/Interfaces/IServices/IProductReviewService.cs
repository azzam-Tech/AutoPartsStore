using AutoPartsStore.Core.Models.Review;

public interface IProductReviewService
{
    Task<List<ProductReviewDto>> GetProductReviewsAsync(int partId, bool? approvedOnly = true);
    Task<List<ProductReviewDto>> GetUserReviewsAsync(int userId);
    Task<List<ProductReviewDto>> GetPendingReviewsAsync();
    Task<ProductReviewDto> GetReviewByIdAsync(int reviewId);
    Task<ProductReviewDto> CreateReviewAsync(int userId, CreateReviewRequest request);
    Task<ProductReviewDto> UpdateReviewAsync(int reviewId, int userId, UpdateReviewRequest request);
    Task<bool> DeleteReviewAsync(int reviewId, int userId);
    Task<bool> ApproveReviewAsync(int reviewId, bool isApproved);
    Task<ReviewSummaryDto> GetReviewSummaryAsync(int partId);

}
using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Review;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IProductReviewRepository : IBaseRepository<ProductReview>
    {
        Task<List<ProductReviewDto>> GetReviewsByPartIdAsync(int partId, ProductReviewstatus? productReviewstatus);
        Task<List<ProductReviewDto>> GetReviewsByUserIdAsync(int userId);
        Task<List<ProductReviewDto>> GetPendingReviewsAsync();
        Task<ProductReviewDto> GetReviewWithDetailsAsync(int reviewId);
        Task<ReviewSummaryDto> GetReviewSummaryAsync(int partId);
        Task<double> GetAverageRatingAsync(int partId);
        Task<int> GetReviewCountAsync(int partId, ProductReviewstatus? productReviewstatus);
        Task<bool> HasUserReviewedPartAsync(int userId, int partId);
        Task<List<ProductReviewDto>> GetReviewsAsync(ProductReviewstatus? productReviewstatus);
    }
}
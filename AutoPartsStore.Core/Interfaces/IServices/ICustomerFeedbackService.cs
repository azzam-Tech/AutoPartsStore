using AutoPartsStore.Core.Models.Feedbacks;

namespace AutoPartsStore.Core.Interfaces.IServices
{
    public interface ICustomerFeedbackService
    {
        Task<List<CustomerFeedbackDto>> GetAllFeedbacksAsync(FeedbackFilterRequest? filter = null);
        Task<CustomerFeedbackDto> GetFeedbackByIdAsync(int id);
        Task<List<CustomerFeedbackDto>> GetUserFeedbacksAsync(int userId);
        Task<CustomerFeedbackDto> CreateFeedbackAsync(int userId, CreateFeedbackRequest request);
        Task<CustomerFeedbackDto> UpdateFeedbackAsync(int id, int userId, UpdateFeedbackRequest request);
        Task<bool> DeleteFeedbackAsync(int id, int userId);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAsUnreadAsync(int id);
        Task<FeedbackStatsDto> GetFeedbackStatsAsync();
        Task<int> GetUnreadCountAsync();
        Task<List<CustomerFeedbackDto>> GetRecentFeedbacksAsync(int count = 10);
        Task<bool> ToggleFeaturedStatusAsync(int id, bool isFeatured);
        Task<List<CustomerFeedbackDto>> GetFeaturedFeedbacksAsync();
    }
}

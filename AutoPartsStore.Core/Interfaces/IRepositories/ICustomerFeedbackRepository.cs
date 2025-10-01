using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Feedbacks;

namespace AutoPartsStore.Core.Interfaces.IRepositories
{
    public interface ICustomerFeedbackRepository : IBaseRepository<CustomerFeedback>
    {
        Task<List<CustomerFeedbackDto>> GetAllWithDetailsAsync(FeedbackFilterRequest filter = null);
        Task<CustomerFeedbackDto> GetByIdWithDetailsAsync(int id);
        Task<List<CustomerFeedbackDto>> GetByUserIdAsync(int userId);
        Task<FeedbackStatsDto> GetFeedbackStatsAsync();
        Task<int> GetUnreadCountAsync();
        Task<List<CustomerFeedbackDto>> GetRecentFeedbacksAsync(int count = 10);
        Task<List<CustomerFeedbackDto>> GetFeaturedFeedbacksAsync();
        Task<bool> ToggleFeaturedStatusAsync(int id, bool isFeatured);
    }
}

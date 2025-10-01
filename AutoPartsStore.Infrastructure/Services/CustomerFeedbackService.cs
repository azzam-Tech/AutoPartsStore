using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Feedbacks;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class CustomerFeedbackService : ICustomerFeedbackService
    {
        private readonly ICustomerFeedbackRepository _feedbackRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerFeedbackService> _logger;

        public CustomerFeedbackService(
            ICustomerFeedbackRepository feedbackRepository,
            AppDbContext context,
            ILogger<CustomerFeedbackService> logger)
        {
            _feedbackRepository = feedbackRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<List<CustomerFeedbackDto>> GetAllFeedbacksAsync(FeedbackFilterRequest filter = null)
        {
            return await _feedbackRepository.GetAllWithDetailsAsync(filter);
        }

        public async Task<CustomerFeedbackDto> GetFeedbackByIdAsync(int id)
        {
            return await _feedbackRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<List<CustomerFeedbackDto>> GetUserFeedbacksAsync(int userId)
        {
            return await _feedbackRepository.GetByUserIdAsync(userId);
        }

        public async Task<CustomerFeedbackDto> CreateFeedbackAsync(int userId, CreateFeedbackRequest request)
        {
            // التحقق من وجود المستخدم
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // التحقق البسيط إذا كان المستخدم قد أضاف فيدباك من قبل
            bool hasExistingFeedback = await _context.CustomerFeedbacks
                .AnyAsync(cf => cf.UserId == userId);

            if (hasExistingFeedback)
                throw new InvalidOperationException("You can only create one feedback");

            var feedback = new CustomerFeedback(
                userId,
                request.FeedbackType,
                request.Message,
                request.Rate
            );

            await _context.CustomerFeedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Feedback created by user {UserId} with type {FeedbackType}",
                userId, request.FeedbackType);

            return await _feedbackRepository.GetByIdWithDetailsAsync(feedback.Id);
        }

        public async Task<CustomerFeedbackDto> UpdateFeedbackAsync(int id, int userId, UpdateFeedbackRequest request)
        {
            var feedback = await _context.CustomerFeedbacks
                .FirstOrDefaultAsync(cf => cf.Id == id && cf.UserId == userId);

            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found or you don't have permission to edit it");

            // استخدام Reflection لتعديل الخصائص الخاصة
            var type = feedback.GetType();

            if (!string.IsNullOrWhiteSpace(request.Message))
                type.GetProperty("Message")?.SetValue(feedback, request.Message);

            if (request.Rate.HasValue)
                type.GetProperty("Rate")?.SetValue(feedback, request.Rate.Value);

            if (request.FeedbackType.HasValue)
                type.GetProperty("FeedbackType")?.SetValue(feedback, request.FeedbackType.Value);

            _context.CustomerFeedbacks.Update(feedback);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Feedback {FeedbackId} updated by user {UserId}", id, userId);
            return await _feedbackRepository.GetByIdWithDetailsAsync(feedback.Id);
        }

        public async Task<bool> DeleteFeedbackAsync(int id, int userId)
        {
            var feedback = await _context.CustomerFeedbacks
                .FirstOrDefaultAsync(cf => cf.Id == id && cf.UserId == userId);

            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found or you don't have permission to delete it");

            _context.CustomerFeedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Feedback {FeedbackId} deleted by user {UserId}", id, userId);
            return true;
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            // لم نعد نستخدم هذه الوظيفة بعد إزالة IsRead
            _logger.LogWarning("MarkAsRead is no longer supported after removing IsRead property");
            return false;
        }

        public async Task<bool> MarkAsUnreadAsync(int id)
        {
            // لم نعد نستخدم هذه الوظيفة بعد إزالة IsRead
            _logger.LogWarning("MarkAsUnread is no longer supported after removing IsRead property");
            return false;
        }

        public async Task<FeedbackStatsDto> GetFeedbackStatsAsync()
        {
            return await _feedbackRepository.GetFeedbackStatsAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            // لم نعد نستخدم هذه الوظيفة
            return 0;
        }

        public async Task<List<CustomerFeedbackDto>> GetRecentFeedbacksAsync(int count = 10)
        {
            return await _feedbackRepository.GetRecentFeedbacksAsync(count);
        }

        public async Task<List<CustomerFeedbackDto>> GetFeaturedFeedbacksAsync()
        {
            return await _feedbackRepository.GetFeaturedFeedbacksAsync();
        }

        public async Task<bool> ToggleFeaturedStatusAsync(int id, bool isFeatured)
        {
            var result = await _feedbackRepository.ToggleFeaturedStatusAsync(id, isFeatured);
            if (result)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Feedback {FeedbackId} featured status: {IsFeatured}", id, isFeatured);
            }
            return result;
        }
    }
}

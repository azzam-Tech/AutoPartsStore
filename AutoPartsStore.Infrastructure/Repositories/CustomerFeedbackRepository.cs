using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Models.Feedbacks;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class CustomerFeedbackRepository : BaseRepository<CustomerFeedback>, ICustomerFeedbackRepository
    {
        public CustomerFeedbackRepository(AppDbContext context) : base(context) { }

        public async Task<List<CustomerFeedbackDto>> GetAllWithDetailsAsync(FeedbackFilterRequest filter = null)
        {
            var query = _context.CustomerFeedbacks
                .Include(cf => cf.User)
                .AsQueryable();

            // تطبيق الفلاتر
            if (filter != null)
            {
                if (filter.FeedbackType.HasValue)
                    query = query.Where(cf => cf.FeedbackType == filter.FeedbackType.Value);

                if (filter.Feedbackstatus.HasValue)
                {
                    if (filter.Feedbackstatus.Value == Feedbackstatus.IsApproved)
                        query = query.Where(cf => cf.IsFeatured == true);
                    else if (filter.Feedbackstatus.Value == Feedbackstatus.IsNotApproved)
                        query = query.Where(cf => !cf.IsFeatured == false);
                    else if (filter.Feedbackstatus.Value == Feedbackstatus.IsPending)
                        query = query.Where(cf => cf.IsFeatured == null);
                }

                if (filter.MinRate.HasValue)
                    query = query.Where(cf => cf.Rate >= filter.MinRate.Value);

                if (filter.MaxRate.HasValue)
                    query = query.Where(cf => cf.Rate <= filter.MaxRate.Value);

                if (filter.FromDate.HasValue)
                    query = query.Where(cf => cf.CreatedDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(cf => cf.CreatedDate <= filter.ToDate.Value);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    query = query.Where(cf => cf.Message.Contains(filter.SearchTerm) ||
                                             cf.User.FullName.Contains(filter.SearchTerm) ||
                                             cf.User.Email.Contains(filter.SearchTerm));
            }

            return await query
                .OrderByDescending(cf => cf.CreatedDate)
                .Select(cf => new CustomerFeedbackDto
                {
                    Id = cf.Id,
                    UserId = cf.UserId,
                    UserName = cf.User.FullName,
                    UserEmail = cf.User.Email,
                    FeedbackType = cf.FeedbackType,
                    FeedbackTypeName = cf.FeedbackType.ToString(),
                    Message = cf.Message,
                    Rate = cf.Rate,
                    RateStars = new string('★', cf.Rate) + new string('☆', 5 - cf.Rate),
                    CreatedDate = cf.CreatedDate,
                    TimeAgo = GetTimeAgo(cf.CreatedDate)
                })
                .ToListAsync();
        }

        public async Task<CustomerFeedbackDto> GetByIdWithDetailsAsync(int id)
        {
            return await _context.CustomerFeedbacks
                .Where(cf => cf.Id == id)
                .Include(cf => cf.User)
                .Select(cf => new CustomerFeedbackDto
                {
                    Id = cf.Id,
                    UserId = cf.UserId,
                    UserName = cf.User.FullName,
                    UserEmail = cf.User.Email,
                    FeedbackType = cf.FeedbackType,
                    FeedbackTypeName = cf.FeedbackType.ToString(),
                    Message = cf.Message,
                    Rate = cf.Rate,
                    RateStars = new string('★', cf.Rate) + new string('☆', 5 - cf.Rate),
                    CreatedDate = cf.CreatedDate,
                    TimeAgo = GetTimeAgo(cf.CreatedDate)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<CustomerFeedbackDto>> GetByUserIdAsync(int userId)
        {
            return await _context.CustomerFeedbacks
                .Where(cf => cf.UserId == userId)
                .Include(cf => cf.User)
                .OrderByDescending(cf => cf.CreatedDate)
                .Select(cf => new CustomerFeedbackDto
                {
                    Id = cf.Id,
                    UserId = cf.UserId,
                    UserName = cf.User.FullName,
                    UserEmail = cf.User.Email,
                    FeedbackType = cf.FeedbackType,
                    FeedbackTypeName = cf.FeedbackType.ToString(),
                    Message = cf.Message,
                    Rate = cf.Rate,
                    RateStars = new string('★', cf.Rate) + new string('☆', 5 - cf.Rate),
                    CreatedDate = cf.CreatedDate,
                    TimeAgo = GetTimeAgo(cf.CreatedDate)
                })
                .ToListAsync();
        }

        public async Task<FeedbackStatsDto> GetFeedbackStatsAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var allFeedbacks = await _context.CustomerFeedbacks.ToListAsync();

            return new FeedbackStatsDto
            {
                TotalFeedbacks = allFeedbacks.Count,
                AverageRating = allFeedbacks.Any() ? allFeedbacks.Average(cf => cf.Rate) : 0,
                FiveStarCount = allFeedbacks.Count(cf => cf.Rate == 5),
                FourStarCount = allFeedbacks.Count(cf => cf.Rate == 4),
                ThreeStarCount = allFeedbacks.Count(cf => cf.Rate == 3),
                TwoStarCount = allFeedbacks.Count(cf => cf.Rate == 2),
                OneStarCount = allFeedbacks.Count(cf => cf.Rate == 1),
                ComplaintsCount = allFeedbacks.Count(cf => cf.FeedbackType == FeedbackType.Complaint),
                SuggestionsCount = allFeedbacks.Count(cf => cf.FeedbackType == FeedbackType.Suggestion),
                InquiriesCount = allFeedbacks.Count(cf => cf.FeedbackType == FeedbackType.Inquiry),
                PraiseCount = allFeedbacks.Count(cf => cf.FeedbackType == FeedbackType.Praise),
                TodayCount = allFeedbacks.Count(cf => cf.CreatedDate >= today),
                ThisWeekCount = allFeedbacks.Count(cf => cf.CreatedDate >= weekStart),
                ThisMonthCount = allFeedbacks.Count(cf => cf.CreatedDate >= monthStart)
            };
        }

        public async Task<int> GetUnreadCountAsync()
        {
            // لم نعد نستخدم IsRead، يمكن إزالة هذه الدالة أو تعديلها
            return 0;
        }

        public async Task<List<CustomerFeedbackDto>> GetRecentFeedbacksAsync(int count = 10)
        {
            return await _context.CustomerFeedbacks
                .Include(cf => cf.User)
                .OrderByDescending(cf => cf.CreatedDate)
                .Take(count)
                .Select(cf => new CustomerFeedbackDto
                {
                    Id = cf.Id,
                    UserId = cf.UserId,
                    UserName = cf.User.FullName,
                    UserEmail = cf.User.Email,
                    FeedbackType = cf.FeedbackType,
                    FeedbackTypeName = cf.FeedbackType.ToString(),
                    Message = cf.Message.Length > 100 ? cf.Message.Substring(0, 100) + "..." : cf.Message,
                    Rate = cf.Rate,
                    RateStars = new string('★', cf.Rate) + new string('☆', 5 - cf.Rate),
                    CreatedDate = cf.CreatedDate,
                    TimeAgo = GetTimeAgo(cf.CreatedDate)
                })
                .ToListAsync();
        }

        private static string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;

            if (timeSpan.TotalMinutes < 1) return "الآن";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} دقيقة";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} ساعة";
            if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays} يوم";

            return $"{(int)(timeSpan.TotalDays / 30)} شهر";
        }

        public async Task<List<CustomerFeedbackDto>> GetFeaturedFeedbacksAsync()
        {
            return await _context.CustomerFeedbacks
                .Where(cf => cf.IsFeatured == true)
                .Include(cf => cf.User)
                .OrderByDescending(cf => cf.CreatedDate)
                .Select(cf => new CustomerFeedbackDto
                {
                    Id = cf.Id,
                    UserId = cf.UserId,
                    UserName = cf.User.FullName,
                    UserEmail = cf.User.Email,
                    FeedbackType = cf.FeedbackType,
                    FeedbackTypeName = cf.FeedbackType.ToString(),
                    Message = cf.Message,
                    Rate = cf.Rate,
                    RateStars = new string('★', cf.Rate) + new string('☆', 5 - cf.Rate),
                    CreatedDate = cf.CreatedDate,
                    TimeAgo = GetTimeAgo(cf.CreatedDate),
                    IsFeatured = cf.IsFeatured
                })
                .ToListAsync();
        }

        public async Task<bool> ToggleFeaturedStatusAsync(int id, bool isFeatured)
        {
            var feedback = await _context.CustomerFeedbacks.FindAsync(id);
            if (feedback == null) return false;

            if (isFeatured)
            {
                feedback.Feature();
            }
            else
            {
                feedback.Unfeature();
            }

            _context.CustomerFeedbacks.Update(feedback);
            return true;
        }
    }
}

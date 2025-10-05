using AutoPartsStore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Feedbacks
{
    public class CustomerFeedbackDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public FeedbackType FeedbackType { get; set; }
        public string FeedbackTypeName { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int Rate { get; set; } // التقييم من 1 إلى 5 نجوم
        public string? RateStars { get; set; } // تمثيل مرئي للنجوم
        public DateTime CreatedDate { get; set; }
        public string TimeAgo { get; set; } = null!;
        public bool? IsFeatured { get; set; }

    }

    public class CreateFeedbackRequest
    {
        [Required]
        public FeedbackType FeedbackType { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
        public string Message { get; set; } = null!;

        [Required]
        [Range(1, 5, ErrorMessage = "Rate must be between 1 and 5")]
        public int Rate { get; set; } = 5; // قيمة افتراضية 5 نجوم
    }

    public class UpdateFeedbackRequest
    {
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
        public string Message { get; set; } = null!;

        [Range(1, 5, ErrorMessage = "Rate must be between 1 and 5")]
        public int? Rate { get; set; }

        public FeedbackType? FeedbackType { get; set; }
    }

    public class FeedbackStatsDto
    {
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public int ComplaintsCount { get; set; }
        public int SuggestionsCount { get; set; }
        public int InquiriesCount { get; set; }
        public int PraiseCount { get; set; }
        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
        public int ThisMonthCount { get; set; }
    }

    public class FeedbackFilterRequest
    {
        public FeedbackType? FeedbackType { get; set; }
        public Feedbackstatus? Feedbackstatus { get; set; }
        public int? MinRate { get; set; }
        public int? MaxRate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; } 
    }

    public class FeatureFeedbackRequest
    {
        [Required]
        public bool IsFeatured { get; set; }
    }
}

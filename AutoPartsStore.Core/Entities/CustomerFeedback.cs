using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Entities
{
    public class CustomerFeedback
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public FeedbackType FeedbackType { get; private set; }

        [Required]
        [MaxLength(2000)]
        public string Message { get; private set; }

        [Range(1, 5)]
        public int Rate { get; private set; } // التقييم من 1 إلى 5 نجوم

        public bool? IsFeatured { get; private set; }

        public DateTime CreatedDate { get; private set; }

        // Navigation
        public User User { get; private set; } = null!;

        public CustomerFeedback(int userId, FeedbackType feedbackType, string message, int rate)
        {
            UserId = userId;
            FeedbackType = feedbackType;
            Message = message;
            Rate = rate;
            CreatedDate = DateTime.UtcNow;
            IsFeatured = null;

            Validate();
        }

        private void Validate()
        {
            if (Rate < 1 || Rate > 5)
                throw new ArgumentException("Rate must be between 1 and 5");

            if (string.IsNullOrWhiteSpace(Message))
                throw new ArgumentException("Message cannot be empty");
        }

        // Methods
        public void UpdateMessage(string newMessage)
        {
            if (string.IsNullOrWhiteSpace(newMessage))
                throw new ArgumentException("Message cannot be empty");

            Message = newMessage;
        }

        public void UpdateRate(int newRate)
        {
            if (newRate < 1 || newRate > 5)
                throw new ArgumentException("Rate must be between 1 and 5");

            Rate = newRate;
        }

        public void ChangeFeedbackType(FeedbackType newType)
        {
            FeedbackType = newType;
        }

        public void Feature()
        {
            IsFeatured = true;
        }

        public void Unfeature()
        {
            IsFeatured = false;
        }
        public void ClearFeature()
        {
            IsFeatured = null;
        }
    }

    public enum FeedbackType
    {
        Complaint = 0,    // شكوى
        Suggestion = 1,   // اقتراح
        Inquiry = 2,      // استفسار
        Praise = 3        // مدح
    }

    public enum Feedbackstatus
    {
        IsApproved = 0,
        IsNotApproved = 1,
        IsPending = 2

    }
}

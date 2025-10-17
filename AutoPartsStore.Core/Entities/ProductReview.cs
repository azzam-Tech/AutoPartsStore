namespace AutoPartsStore.Core.Entities
{
    public class ProductReview
    {
        public int Id { get; private set; }
        public int PartId { get; private set; }
        public int? UserId { get; private set; }
        public int Rating { get; private set; } // 1-5
        public string? ReviewText { get; private set; }
        public DateTime ReviewDate { get; private set; }
        public bool? IsApproved { get; private set; } // null = Pending, true = Approved, false = Rejected

        // Navigation
        public CarPart CarPart { get; private set; } = null!;
        public User User { get; private set; } = null!;

        public ProductReview(int partId, int? userId, int rating, string? reviewText = null)
        {
            PartId = partId;
            UserId = userId;
            Rating = rating;
            ReviewText = reviewText;
            ReviewDate = DateTime.UtcNow;
            IsApproved = null; // Default to pending

            Validate();
        }

        private void Validate()
        {
            if (Rating < 1 || Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");
        }

        public void Approve() => IsApproved = true;
        public void Reject() => IsApproved = false;
        public void SetPending() => IsApproved = null;
        
        public string GetApprovalStatus()
        {
            return IsApproved switch
            {
                true => "Approved",
                false => "Rejected",
                null => "Pending"
            };
        }
    }

    public enum ProductReviewstatus
    {
        IsApproved = 0,
        IsNotApproved = 1,
        IsPending = 2
    }
}

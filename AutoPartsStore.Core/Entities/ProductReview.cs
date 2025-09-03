namespace AutoPartsStore.Core.Entities
{
    public class ProductReview 
    {
        public int Id { get; private set; }
        public int PartId { get; private set; }
        public int UserId { get; private set; }
        public int Rating { get; private set; } // 1-5
        public string? ReviewText { get; private set; }
        public DateTime ReviewDate { get; private set; }
        public bool IsApproved { get; private set; }

        // Navigation
        public CarPart CarPart { get; private set; }
        public User User { get; private set; }

        public ProductReview(int partId, int userId, int rating, string? reviewText = null)
        {
            PartId = partId;
            UserId = userId;
            Rating = rating;
            ReviewText = reviewText;
            ReviewDate = DateTime.UtcNow;
            IsApproved = false;

            Validate();
        }

        private void Validate()
        {
            if (Rating < 1 || Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");
        }

        public void Approve() => IsApproved = true;
        public void Reject() => IsApproved = false;
    }
}

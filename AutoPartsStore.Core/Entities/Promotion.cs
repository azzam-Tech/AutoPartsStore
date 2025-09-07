namespace AutoPartsStore.Core.Entities
{
    public class Promotion
    {
        public int Id { get; private set; }
        public string PromotionName { get; private set; }
        public string? Description { get; private set; }
        public DiscountType DiscountType { get; private set; }  // Enum stored as int
        public decimal DiscountValue { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsActive { get; private set; }
        public decimal MinOrderAmount { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }


        // Relationship
        public List<ProductPromotion> ProductPromotions { get; private set; } = new();

        public Promotion(
            string promotionName, DiscountType discountType, decimal discountValue,
            DateTime startDate, DateTime endDate,
            decimal minOrderAmount = 0, string? description = null)
        {
            PromotionName = promotionName;
            DiscountType = discountType;
            DiscountValue = discountValue;
            StartDate = startDate;
            EndDate = endDate;
            MinOrderAmount = minOrderAmount;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;

            Validate();
        }

        private void Validate()
        {
            if (DiscountValue < 0)
                throw new ArgumentException("DiscountValue must be >= 0");
            if (StartDate >= EndDate)
                throw new ArgumentException("Start date must be before end date");
        }

        public bool IsActiveNow() => IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
        }
    }

    public enum DiscountType
    {
        Percent = 0,
        Fixed = 1
    }
}

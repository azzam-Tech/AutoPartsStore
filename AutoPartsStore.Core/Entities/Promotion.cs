namespace AutoPartsStore.Core.Entities
{
    public class Promotion 
    {
        public int Id { get; private set; }
        public string PromotionName { get; private set; }
        public string? Description { get; private set; }
        public char DiscountType { get; private set; } // 'P' for Percent, 'F' for Fixed
        public decimal DiscountValue { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsActive { get; private set; }
        public decimal MinOrderAmount { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }


        // Relationship
        public List<ProductPromotion> ProductPromotions { get; private set; } = new();

        public Promotion(
            string promotionName, char discountType, decimal discountValue,
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

            Validate();
        }

        private void Validate()
        {
            if (DiscountType != 'P' && DiscountType != 'F')
                throw new ArgumentException("DiscountType must be 'P' or 'F'");
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
}

namespace AutoPartsStore.Core.Entities
{
    public class ProductPromotion
    {
        public int Id { get; private set; }
        public int PromotionId { get; private set; }
        public int PartId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; } // Added as requested

        // Navigation
        public Promotion Promotion { get; private set; }
        public CarPart CarPart { get; private set; }

        public ProductPromotion(int promotionId, int partId)
        {
            PromotionId = promotionId;
            PartId = partId;
            CreatedAt = DateTime.UtcNow;
        }

        // Method to update timestamp
        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
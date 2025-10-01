using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Core.Models.Promotions
{
    public class PromotionDto
    {
        public int Id { get; set; }
        public string PromotionName { get; set; } = null!;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public decimal MinOrderAmount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActiveNow { get; set; }
        public int ProductCount { get; set; }
    }
}
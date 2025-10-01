using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Core.Models.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int PartId { get; set; }
        public string PartNumber { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool HasPromotion { get; set; }
        public string? PromotionName { get; set; }
        public DiscountType? DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal FinalPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal FinalTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAvailable { get; set; }
        public int AvailableStock { get; set; }
    }
}
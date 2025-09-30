using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Core.Models.Pricing
{
    public class ProductPriceDto
    {
        public decimal UnitPrice { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; } // يمكن أن تكون نسبة أو مبلغ ثابت
        public decimal FinalUnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }
    }

    public class CartItemPriceRequest
    {
        public decimal UnitPrice { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public int Quantity { get; set; }
    }

    public class CartPriceSummaryDto
    {
        public decimal TotalPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal FinalTotal { get; set; }
        public int TotalItems { get; set; }
    }
}

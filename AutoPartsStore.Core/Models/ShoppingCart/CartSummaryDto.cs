namespace AutoPartsStore.Core.Models.Cart
{
    public class CartSummaryDto
    {
        public int TotalItems { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal FinalTotal { get; set; }
    }
}
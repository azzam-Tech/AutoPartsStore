using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Core.Models.Promotions
{
    public class PromotionFilter
    {
        public bool? isActive { get; set; }
        public DiscountType? discountType { get; set; }
        public int pageNum { get; set; } = 1;
        public int pageSize { get; set; } = 10;

    }
}
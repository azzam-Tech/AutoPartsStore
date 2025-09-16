using AutoPartsStore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Promotions
{
    public class UpdatePromotionRequest
    {
        [StringLength(100)]
        public string PromotionName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DiscountType? DiscountType { get; set; }

        [Range(0, 100000)]
        public decimal? DiscountValue { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0, 100000)]
        public decimal? MinOrderAmount { get; set; }

        public bool? IsActive { get; set; }
    }
}
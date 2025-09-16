using AutoPartsStore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Promotions
{
    public class CreatePromotionRequest
    {
        [Required]
        [StringLength(100)]
        public string PromotionName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(0, 100000)]
        public decimal MinOrderAmount { get; set; }
    }
}
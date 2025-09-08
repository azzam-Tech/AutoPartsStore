using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.CarPart
{
    public class UpdateCarPartRequest
    {
        [Required]
        [StringLength(200)]
        public string PartName { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(100)]
        public string CarBrand { get; set; }

        [StringLength(100)]
        public string CarModel { get; set; }

        [StringLength(100)]
        public string CarYear { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Url]
        [StringLength(255)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; }
    }
}
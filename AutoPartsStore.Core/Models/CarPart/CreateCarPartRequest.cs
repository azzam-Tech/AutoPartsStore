using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.CarPart
{
    public class CreateCarPartRequest
    {
        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; }

        [Required]
        public int CategoryId { get; set; }

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
        public decimal DiscountPercent { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } = 0;

        [Url]
        [StringLength(255)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
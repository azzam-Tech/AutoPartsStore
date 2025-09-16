using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Core.Models.Promotions
{
    public class ProductPromotionDto
    {
        public int Id { get; set; }
        public int PromotionId { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public PromotionDto? PromotionDto { get; set; }
    }
    public class PartPromotionDto
    {
        public int PartId { get; set; }
        public int PromotionId { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
namespace AutoPartsStore.Core.Models.CarPart
{
    public class CarPartDto
    {
        public int Id { get; set; }
        public string? PartNumber { get; set; }
        public string? PartName { get; set; }
        public string? Description { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public string? CarYear { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool IsInStock { get; set; }
        public bool IsOnSale { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Category info
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Statistics
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int TotalSold { get; set; }
    }
}
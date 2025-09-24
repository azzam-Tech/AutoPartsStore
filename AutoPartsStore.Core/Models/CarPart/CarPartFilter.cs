namespace AutoPartsStore.Core.Models.CarPart
{
    public class CarPartFilter
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsActive { get; set; }
        public bool? InStock { get; set; }
        public bool? OnSale { get; set; }
        public SortBy? SortBy { get; set; }
        public bool? SortDescending { get; set; }
        public bool? TodaysOffers { get; set; }
        public bool? BestSellers { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
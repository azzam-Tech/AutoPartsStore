namespace AutoPartsStore.Core.Models.Favorites
{
    public class FavoriteDto
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public bool IsInStock { get; set; }
        public DateTime AddedDate { get; set; }
    }
}

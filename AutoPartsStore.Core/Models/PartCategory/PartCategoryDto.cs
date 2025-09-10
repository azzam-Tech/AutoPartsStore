namespace AutoPartsStore.Core.Models.PartCategory
{
    public class PartCategoryDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int ProductsCount { get; set; }
        public List<PartCategoryDto>? SubCategories { get; set; } = new();
    }
}
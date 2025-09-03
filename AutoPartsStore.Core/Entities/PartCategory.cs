namespace AutoPartsStore.Core.Entities
{
    public class PartCategory 
    {
        public int Id { get; private set; }
        public string CategoryName { get; private set; }
        public int? ParentCategoryId { get; private set; }
        public string? Description { get; private set; }
        public string? ImageUrl { get; private set; }
        public bool IsActive { get; private set; }

        // Relationships
        public PartCategory? ParentCategory { get; private set; }
        public List<PartCategory> SubCategories { get; private set; } = new();
        public List<CarPart> CarParts { get; private set; } = new();

        public PartCategory(string categoryName, int? parentCategoryId = null, string? description = null, string? imageUrl = null)
        {
            CategoryName = categoryName;
            ParentCategoryId = parentCategoryId;
            Description = description;
            ImageUrl = imageUrl;
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}

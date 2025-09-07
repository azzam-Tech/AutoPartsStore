namespace AutoPartsStore.Core.Entities
{
    public class CarPart 
    {
        public int Id { get; private set; }
        public string PartNumber { get; private set; }
        public int CategoryId { get; private set; }
        public string PartName { get; private set; }
        public string? Description { get; private set; }
        public string? CarBrand { get; private set; }
        public string? CarModel { get; private set; }
        public string? CarYear { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal DiscountPercent { get; private set; }
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string? ImageUrl { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }


        // Relationships
        public PartCategory Category { get; private set; }
        public List<PartSupply> Supplies { get; private set; } = new();
        public List<ProductPromotion> ProductPromotions { get; private set; } = new();
        public List<CartItem> CartItems { get; private set; } = new();
        public List<ProductReview> Reviews { get; private set; } = new();
        public List<InventoryLog> InventoryLogs { get; private set; } = new();

        public CarPart(
            string partNumber, int categoryId, string partName,
            decimal unitPrice, int stockQuantity,
            string? description = null, string? carBrand = null,
            string? carModel = null, string? carYear = null,
            decimal discountPercent = 0, string? imageUrl = null)
        {
            PartNumber = partNumber;
            CategoryId = categoryId;
            PartName = partName;
            UnitPrice = unitPrice;
            StockQuantity = stockQuantity;
            Description = description;
            CarBrand = carBrand;
            CarModel = carModel;
            CarYear = carYear;
            DiscountPercent = discountPercent;
            ImageUrl = imageUrl;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
            DeletedAt = null;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0) throw new ArgumentException("Price must be greater than zero");
            UnitPrice = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ReduceStock(int quantity)
        {
            if (quantity > StockQuantity)
                throw new InvalidOperationException("Not enough stock");
            StockQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncreaseStock(int quantity)
        {
            StockQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public decimal GetFinalPrice()
        {
            return UnitPrice * (1 - DiscountPercent / 100);
        }
        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
        }
    }
}

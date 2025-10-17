using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Entities
{
    public class CarPart
    {
        public int Id { get; private set; }

        [Required]
        [MaxLength(50)]
        public string PartNumber { get; private set; }

        public int CategoryId { get; private set; }
        public int? PromotionId { get; private set; } // العلاقة الجديدة


        [Required]
        [MaxLength(200)]
        public string PartName { get; private set; }

        [MaxLength(1000)]
        public string? Description { get; private set; }

        [MaxLength(100)]
        public string? CarBrand { get; private set; }

        [MaxLength(100)]
        public string? CarModel { get; private set; }

        [MaxLength(100)]
        public string? CarYear { get; private set; }

        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; private set; }
        public decimal FinalPrice { get; private set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; private set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; private set; }

        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        [Url]
        [MaxLength(255)]
        public string? ImageUrl { get; private set; }

        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        // Relationships
        public PartCategory Category { get; private set; }
        public List<PartSupply> Supplies { get; private set; } = new();
        public List<CartItem> CartItems { get; private set; } = new();
        public List<ProductReview> Reviews { get; private set; } = new();
        public List<InventoryLog> InventoryLogs { get; private set; } = new();
        public Promotion? Promotion { get; private set; }


        public CarPart(
            string partNumber, int categoryId, string partName,
            decimal unitPrice, int stockQuantity,
            string? description = null, string? carBrand = null,
            string? carModel = null, string? carYear = null,
            decimal discountPercent = 0, string? imageUrl = null,
            int? promotionId = null)
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
            PromotionId = promotionId;

        }

        // Methods
        public void UpdateBasicInfo(string partName, string? description,
                                  string? carBrand, string? carModel, string? carYear)
        {
            PartName = partName;
            Description = description;
            CarBrand = carBrand;
            CarModel = carModel;
            CarYear = carYear;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0)
                throw new ArgumentException("Price must be greater than zero");

            UnitPrice = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDiscount(decimal discountPercent)
        {
            if (discountPercent < 0 || discountPercent > 100)
                throw new ArgumentException("Discount must be between 0 and 100");

            DiscountPercent = discountPercent;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStock(int newQuantity)
        {
            if (newQuantity < 0)
                throw new ArgumentException("Stock cannot be negative");

            StockQuantity = newQuantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ReduceStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            if (quantity > StockQuantity)
                throw new InvalidOperationException("Not enough stock");

            StockQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            StockQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        // Alias for compatibility
        public void AddStock(int quantity) => IncreaseStock(quantity);

        public void UpdateImage(string imageUrl)
        {
            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeCategory(int newCategoryId)
        {
            CategoryId = newCategoryId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public decimal GetFinalPrice()
        {
            if (DiscountPercent > 0)
                return UnitPrice * (1 - DiscountPercent / 100);
            else
                return FinalPrice;
        }

        public void UpdateFinalPrice(decimal finalPrice)
        {
            if (finalPrice < 0)
                throw new ArgumentException("Final price cannot be negative");

            FinalPrice = finalPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignPromotion(int? promotionId)
        {
            PromotionId = promotionId;
            UpdatedAt = DateTime.UtcNow;
        }
        public void RemovePromotion()
        {
            PromotionId = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsInStock() => StockQuantity > 0;

        public bool IsOnSale() => DiscountPercent > 0;
    }
}
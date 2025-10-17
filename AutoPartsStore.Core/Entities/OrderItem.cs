namespace AutoPartsStore.Core.Entities
{
    /// <summary>
    /// Order item entity - represents individual products in an order
    /// Uses unified pricing logic: Product discount has PRIORITY over promotion
    /// </summary>
    public class OrderItem
    {
        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public int PartId { get; private set; }
        
        // Product snapshot at time of order
        public string PartNumber { get; private set; }
        public string PartName { get; private set; }
        public string? ImageUrl { get; private set; }
        
        // Pricing information - Making public for external calculations
        public decimal UnitPrice { get; set; }           // Original unit price
        public decimal DiscountPercent { get; set; }     // Product's direct discount
        public int Quantity { get; set; }
        
        // Promotion/Offer information (if applicable)
        public int? PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public DiscountType? PromotionDiscountType { get; set; }
        public decimal? PromotionDiscountValue { get; set; }
        
        // Calculated amounts - PUBLIC for external access
        public decimal SubTotal { get; set; }            // UnitPrice * Quantity (TotalPrice)
        public decimal DiscountAmount { get; set; }      // Total discount applied (TotalDiscount)
        public decimal FinalPrice { get; set; }          // Price per unit after discount
        public decimal TotalAmount { get; set; }         // Final total after discount (FinalTotal)
        
        // Audit
        public DateTime CreatedAt { get; private set; }
        
        // Navigation properties
        public Order Order { get; private set; } = null!;
        public CarPart CarPart { get; private set; } = null!;

        // Constructor
        public OrderItem(
            int orderId,
            int partId,
            string partNumber,
            string partName,
            decimal unitPrice,
            decimal discountPercent,
            int quantity,
            string? imageUrl = null,
            int? promotionId = null,
            string? promotionName = null,
            DiscountType? promotionDiscountType = null,
            decimal? promotionDiscountValue = null)
        {
            OrderId = orderId;
            PartId = partId;
            PartNumber = partNumber;
            PartName = partName;
            UnitPrice = unitPrice;
            DiscountPercent = discountPercent;
            Quantity = quantity;
            ImageUrl = imageUrl;
            
            // Promotion information
            PromotionId = promotionId;
            PromotionName = promotionName;
            PromotionDiscountType = promotionDiscountType;
            PromotionDiscountValue = promotionDiscountValue;
            
            CreatedAt = DateTime.UtcNow;
            
            // Calculate amounts using unified pricing logic
            CalculateAmounts();
            Validate();
        }

        /// <summary>
        /// Calculate all amounts using UNIFIED pricing logic
        /// RULE: Product discount has PRIORITY over promotion
        /// 
        /// Definitions:
        /// - UnitPrice: Original product price
        /// - FinalPrice: Price per unit after best discount
        /// - SubTotal: UnitPrice * Quantity (TotalPrice)
        /// - DiscountAmount: Total discount amount (TotalDiscount)
        /// - TotalAmount: SubTotal - DiscountAmount (FinalTotal)
        /// </summary>
        private void CalculateAmounts()
        {
            // SubTotal = TotalPrice (before any discount)
            SubTotal = UnitPrice * Quantity;
            
            // Calculate FinalPrice using PRIORITY rule: Product discount > Promotion
            if (DiscountPercent > 0)
            {
                // Product has discount - USE IT (has priority)
                FinalPrice = UnitPrice * (1 - DiscountPercent / 100);
            }
            else if (PromotionId.HasValue && PromotionDiscountType.HasValue && PromotionDiscountValue.HasValue)
            {
                // No product discount, use promotion
                if (PromotionDiscountType == DiscountType.Percent)
                {
                    FinalPrice = UnitPrice * (1 - PromotionDiscountValue.Value / 100);
                }
                else if (PromotionDiscountType == DiscountType.Fixed)
                {
                    FinalPrice = Math.Max(0, UnitPrice - PromotionDiscountValue.Value);
                }
                else
                {
                    FinalPrice = UnitPrice;
                }
            }
            else
            {
                // No discount at all
                FinalPrice = UnitPrice;
            }
            
            // TotalAmount = FinalTotal (with quantity and discount)
            TotalAmount = FinalPrice * Quantity;
            
            // DiscountAmount = TotalDiscount
            DiscountAmount = SubTotal - TotalAmount;
        }

        private void Validate()
        {
            if (Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");
            if (UnitPrice < 0)
                throw new ArgumentException("UnitPrice cannot be negative");
            if (DiscountPercent < 0 || DiscountPercent > 100)
                throw new ArgumentException("DiscountPercent must be between 0 and 100");
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");
                
            Quantity = newQuantity;
            CalculateAmounts();
        }

        /// <summary>
        /// Recalculate all pricing amounts (useful after external changes)
        /// </summary>
        public void RecalculateAmounts()
        {
            CalculateAmounts();
        }
    }
}

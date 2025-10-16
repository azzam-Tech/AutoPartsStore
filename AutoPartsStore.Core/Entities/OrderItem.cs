namespace AutoPartsStore.Core.Entities
{
    /// <summary>
    /// Order item entity - represents individual products in an order
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
        
        // Pricing information
        public decimal UnitPrice { get; private set; }           // Original unit price
        public decimal DiscountPercent { get; private set; }     // Product's direct discount
        public int Quantity { get; private set; }
        
        // Promotion/Offer information (if applicable)
        public int? PromotionId { get; private set; }
        public string? PromotionName { get; private set; }
        public DiscountType? PromotionDiscountType { get; private set; }
        public decimal? PromotionDiscountValue { get; private set; }
        
        // Calculated amounts
        public decimal SubTotal { get; private set; }            // UnitPrice * Quantity
        public decimal DiscountAmount { get; private set; }      // Total discount applied
        public decimal FinalPrice { get; private set; }          // Price per unit after all discounts
        public decimal TotalAmount { get; private set; }         // Final amount for this line item
        
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
            
            // Calculate amounts
            CalculateAmounts();
            Validate();
        }

        private void CalculateAmounts()
        {
            SubTotal = UnitPrice * Quantity;
            
            decimal priceAfterProductDiscount = UnitPrice;
            
            // Apply product's direct discount
            if (DiscountPercent > 0)
            {
                priceAfterProductDiscount = UnitPrice * (1 - DiscountPercent / 100);
            }
            
            // Apply promotion discount if exists and is better
            decimal priceAfterPromotion = priceAfterProductDiscount;
            if (PromotionId.HasValue && PromotionDiscountType.HasValue && PromotionDiscountValue.HasValue)
            {
                if (PromotionDiscountType == DiscountType.Percent)
                {
                    priceAfterPromotion = UnitPrice * (1 - PromotionDiscountValue.Value / 100);
                }
                else if (PromotionDiscountType == DiscountType.Fixed)
                {
                    priceAfterPromotion = Math.Max(0, UnitPrice - PromotionDiscountValue.Value);
                }
                
                // Use the better discount (lower price)
                FinalPrice = Math.Min(priceAfterProductDiscount, priceAfterPromotion);
            }
            else
            {
                FinalPrice = priceAfterProductDiscount;
            }
            
            TotalAmount = FinalPrice * Quantity;
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
    }
}

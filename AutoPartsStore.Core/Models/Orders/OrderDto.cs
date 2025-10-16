using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Core.Models.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        
        // Shipping address
        public int ShippingAddressId { get; set; }
        public string ShippingAddress { get; set; } = null!;
        
        // Order amounts
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Status
        public OrderStatus Status { get; set; }
        public string StatusText { get; set; } = null!;
        
        // Dates
        public DateTime OrderDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        
        // Payment
        public int? PaymentTransactionId { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        
        // Additional info
        public string? CustomerNotes { get; set; }
        public string? AdminNotes { get; set; }
        public string? TrackingNumber { get; set; }
        
        // Order items
        public List<OrderItemDto> Items { get; set; } = new();
        
        // Counts
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int PartId { get; set; }
        public string PartNumber { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public string? ImageUrl { get; set; }
        
        // Pricing
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public int Quantity { get; set; }
        
        // Promotion info
        public int? PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public string? PromotionDiscountType { get; set; }
        public decimal? PromotionDiscountValue { get; set; }
        
        // Calculated amounts
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public OrderStatus Status { get; set; }
        public string StatusText { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }
}

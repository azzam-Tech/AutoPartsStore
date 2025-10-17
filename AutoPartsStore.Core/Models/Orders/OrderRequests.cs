using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Orders
{
    /// <summary>
    /// Request model for creating order from shopping cart
    /// </summary>
    public class CreateOrderFromCartRequest
    {
        [Required]
        public int ShippingAddressId { get; set; }
        
        public string? CustomerNotes { get; set; }
    }

    /// <summary>
    /// Request model for creating order directly (without cart)
    /// </summary>
    public class CreateOrderRequest
    {
        [Required]
        public int ShippingAddressId { get; set; }
        
        public string? CustomerNotes { get; set; }
        
        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        [Required]
        public int PartId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        [Required]
        public int OrderStatus { get; set; }
        
        public string? Notes { get; set; }
    }

    public class CancelOrderRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 5)]
        public string Reason { get; set; } = null!;
    }

    public class UpdateTrackingRequest
    {
        [Required]
        [StringLength(100)]
        public string TrackingNumber { get; set; } = null!;
    }

    public class OrderFilterRequest
    {
        public int? UserId { get; set; }
        public int? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? SearchTerm { get; set; }  // Search by order number or user
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

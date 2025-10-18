using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Payments
{
    /// <summary>
    /// Request to checkout shopping cart and initiate payment
    /// </summary>
    public class CheckoutCartRequest
    {
        [Required]
        public int CartId { get; set; }

        // Customer information (from authenticated user or form)
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        // Optional: Shipping address ID if applicable
        public int? ShippingAddressId { get; set; }

        // Payment method selection
        [Required]
        public Core.Entities.PaymentMethod PaymentMethod { get; set; }

        // Optional: Custom redirect URLs
        public string? CustomRedirectUrl { get; set; }
        public string? CustomWebhookUrl { get; set; }
    }

    /// <summary>
    /// Response after checkout containing Tap payment URL
    /// </summary>
    public class CheckoutResponse
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public int PaymentId { get; set; }
        public string TapChargeId { get; set; } = null!;
        public string TransactionUrl { get; set; } = null!;  // URL to redirect customer to
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string Status { get; set; } = null!;
    }
}

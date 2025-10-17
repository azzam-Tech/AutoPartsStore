using AutoPartsStore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Payments
{
    public class InitiatePaymentRequest
    {
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        // For credit card payments
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? CVC { get; set; }  // Changed from CVV to CVC for consistency
        
        // For Apple Pay
        public string? ApplePayToken { get; set; }  // ? NEW: Token from Apple Pay SDK
        
        // Callback URL for Moyasar
        public string? CallbackUrl { get; set; }
    }

    public class ProcessPaymentCallbackRequest
    {
        [Required]
        public string PaymentId { get; set; } = null!;
        
        public string? Status { get; set; }
        public string? Message { get; set; }
    }

    public class RefundPaymentRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = null!;
    }

    public class PaymentFilterRequest
    {
        public int? UserId { get; set; }
        public int? OrderId { get; set; }
        public PaymentStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

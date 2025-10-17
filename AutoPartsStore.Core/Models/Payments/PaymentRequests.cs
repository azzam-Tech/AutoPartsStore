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
        
        // For card payments - Tap Token from Tap.js
        // The frontend will tokenize the card using Tap.js and send the token
        public string? TapToken { get; set; }  // Token from Tap.js (tok_xxxx)
        
        // For Apple Pay
        public string? ApplePayToken { get; set; }  // Token from Apple Pay SDK
        
        // Customer information (required by Tap)
        [Required]
        public string FirstName { get; set; } = null!;
        
        public string? MiddleName { get; set; }
        
        [Required]
        public string LastName { get; set; } = null!;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;
        
        // Callback URLs
        public string? RedirectUrl { get; set; }
        public string? WebhookUrl { get; set; }
    }

    public class ProcessPaymentWebhookRequest
    {
        [Required]
        public string ChargeId { get; set; } = null!;
        
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

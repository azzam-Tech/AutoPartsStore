using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Core.Models.Payments
{
    public class PaymentTransactionDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        
        public string? TapChargeId { get; set; }  // Updated from Moyasar
        public string TransactionReference { get; set; } = null!;
        
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodText { get; set; } = null!;
        public PaymentStatus Status { get; set; }
        public string StatusText { get; set; } = null!;
        
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        
        public string? AuthorizationCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
        public string? CardScheme { get; set; }  // New: Card scheme (Visa, MasterCard, Mada)
        
        public decimal? RefundedAmount { get; set; }
        public DateTime? RefundedDate { get; set; }
        public string? RefundReason { get; set; }
        
        public DateTime InitiatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? FailedDate { get; set; }
    }

    public class PaymentSummaryDto
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public int RefundedPayments { get; set; }
        public decimal TotalRefunded { get; set; }
    }
}

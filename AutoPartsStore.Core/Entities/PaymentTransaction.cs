namespace AutoPartsStore.Core.Entities
{
    /// <summary>
    /// Payment status enum matching Tap payment statuses
    /// </summary>
    public enum PaymentStatus
    {
        Initiated = 0,      // Payment created but not processed
        Pending = 1,        // Payment processing (IN_PROGRESS)
        Paid = 2,           // Payment successful (CAPTURED)
        Failed = 3,         // Payment failed
        Authorized = 4,     // Payment authorized but not captured
        Captured = 5,       // Payment captured (same as Paid)
        Refunded = 6,       // Full refund
        PartiallyRefunded = 7,  // Partial refund
        Voided = 8,         // Payment voided
        Declined = 9,       // Payment declined by bank
        Abandoned = 10,     // Payment abandoned by user
        Cancelled = 11      // Payment cancelled
    }

    /// <summary>
    /// Payment method enum
    /// </summary>
    public enum PaymentMethod
    {
        Visa = 0,           // Visa cards
        MasterCard = 1,     // MasterCard
        Mada = 2,           // Saudi Mada cards
        ApplePay = 3,       // Apple Pay
        Tabby = 4,          // Tabby (Buy now, pay later)
        Tamara = 5,         // Tamara (Buy now, pay later) - Not in initial scope
        STCPay = 6          // STC Pay - Not in initial scope
    }

    /// <summary>
    /// Payment transaction entity - Updated for Tap
    /// </summary>
    public class PaymentTransaction
    {
        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public int UserId { get; private set; }
        
        // Tap payment details
        public string? TapChargeId { get; private set; }     // Tap's charge ID (chg_xxx)
        public string TransactionReference { get; private set; } // Our reference
        
        // Payment information
        public PaymentMethod PaymentMethod { get; private set; }
        public PaymentStatus Status { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "SAR";   // Saudi Riyal
        
        // Payment gateway response
        public string? GatewayResponse { get; private set; }    // Full JSON response
        public string? AuthorizationCode { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? ErrorCode { get; private set; }
        
        // Card information (last 4 digits only for security)
        public string? CardLast4 { get; private set; }
        public string? CardBrand { get; private set; }          // Visa, MasterCard, Mada
        public string? CardScheme { get; private set; }         // Card scheme
        
        // Refund information
        public decimal? RefundedAmount { get; private set; }
        public DateTime? RefundedDate { get; private set; }
        public string? RefundReason { get; private set; }
        public string? RefundReference { get; private set; }    // Tap refund ID
        
        // Timestamps
        public DateTime InitiatedDate { get; private set; }
        public DateTime? CompletedDate { get; private set; }
        public DateTime? FailedDate { get; private set; }
        
        // Audit
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        
        // Navigation properties
        public Order Order { get; private set; } = null!;
        public User User { get; private set; } = null!;

        // Constructor
        public PaymentTransaction(
            int orderId,
            int userId,
            decimal amount,
            PaymentMethod paymentMethod,
            string? tapChargeId = null)
        {
            OrderId = orderId;
            UserId = userId;
            Amount = amount;
            PaymentMethod = paymentMethod;
            TapChargeId = tapChargeId;
            
            TransactionReference = GenerateTransactionReference();
            Status = PaymentStatus.Initiated;
            InitiatedDate = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;

            Validate();
        }

        private void Validate()
        {
            if (Amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero");
        }

        // Methods
        public void UpdateStatus(
            PaymentStatus newStatus,
            string? gatewayResponse = null,
            string? authorizationCode = null)
        {
            Status = newStatus;
            GatewayResponse = gatewayResponse;
            AuthorizationCode = authorizationCode;
            UpdatedAt = DateTime.UtcNow;

            switch (newStatus)
            {
                case PaymentStatus.Paid:
                case PaymentStatus.Captured:
                    CompletedDate = DateTime.UtcNow;
                    break;
                case PaymentStatus.Failed:
                case PaymentStatus.Declined:
                    FailedDate = DateTime.UtcNow;
                    break;
            }
        }

        public void MarkAsFailed(string errorMessage, string? errorCode = null)
        {
            Status = PaymentStatus.Failed;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            FailedDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateCardDetails(string last4, string brand, string? scheme = null)
        {
            CardLast4 = last4;
            CardBrand = brand;
            CardScheme = scheme;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ProcessRefund(decimal refundAmount, string reason, string? refundReference = null)
        {
            if (refundAmount <= 0)
                throw new ArgumentException("Refund amount must be greater than zero");
            if (refundAmount > Amount)
                throw new ArgumentException("Refund amount cannot exceed payment amount");

            RefundedAmount = (RefundedAmount ?? 0) + refundAmount;
            RefundedDate = DateTime.UtcNow;
            RefundReason = reason;
            RefundReference = refundReference;
            
            if (RefundedAmount >= Amount)
            {
                Status = PaymentStatus.Refunded;
            }
            else
            {
                Status = PaymentStatus.PartiallyRefunded;
            }
            
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTapChargeId(string tapChargeId)
        {
            TapChargeId = tapChargeId;
            UpdatedAt = DateTime.UtcNow;
        }

        private static string GenerateTransactionReference()
        {
            // Format: TXN-YYYYMMDD-XXXXX (e.g., TXN-20250105-12345)
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = new Random().Next(10000, 99999);
            return $"TXN-{datePart}-{randomPart}";
        }

        public bool IsSuccessful()
        {
            return Status == PaymentStatus.Paid || Status == PaymentStatus.Captured;
        }

        public bool CanBeRefunded()
        {
            return (Status == PaymentStatus.Paid || Status == PaymentStatus.Captured) &&
                   (RefundedAmount == null || RefundedAmount < Amount);
        }
    }
}

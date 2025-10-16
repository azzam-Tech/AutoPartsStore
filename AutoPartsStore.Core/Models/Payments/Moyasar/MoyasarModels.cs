namespace AutoPartsStore.Core.Models.Payments.Moyasar
{
    /// <summary>
    /// Moyasar payment request model
    /// Reference: https://docs.moyasar.com/api/payments/01-create-payment
    /// </summary>
    public class MoyasarCreatePaymentRequest
    {
        public decimal Amount { get; set; }  // Amount in halalas (smallest currency unit)
        public string Currency { get; set; } = "SAR";
        public string Description { get; set; } = null!;
        public MoyasarSource Source { get; set; } = null!;
        public string? CallbackUrl { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class MoyasarSource
    {
        public string Type { get; set; } = null!;  // "creditcard", "applepay", "stcpay", etc.
        public string? Number { get; set; }  // Card number
        public string? Name { get; set; }    // Cardholder name
        public string? Month { get; set; }   // Expiry month
        public string? Year { get; set; }    // Expiry year
        public string? Cvc { get; set; }     // CVV
        public string? Company { get; set; } // For Tabby/Tamara
        public string? Token { get; set; }   // For ApplePay
    }

    /// <summary>
    /// Moyasar payment response
    /// </summary>
    public class MoyasarPaymentResponse
    {
        public string Id { get; set; } = null!;
        public string Status { get; set; } = null!;  // "paid", "failed", "initiated", etc.
        public int Amount { get; set; }
        public int Fee { get; set; }
        public string Currency { get; set; } = null!;
        public int RefundedAmount { get; set; }
        public int CapturedAmount { get; set; }
        public string? RefundedAt { get; set; }
        public string? CapturedAt { get; set; }
        public string? VoidedAt { get; set; }
        public string Description { get; set; } = null!;
        public string? InvoiceId { get; set; }
        public string? Ip { get; set; }
        public string? CallbackUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public MoyasarSourceResponse? Source { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class MoyasarSourceResponse
    {
        public string Type { get; set; } = null!;
        public string? Company { get; set; }
        public string? Name { get; set; }
        public string? Number { get; set; }  // Masked card number
        public string? GatewayId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public MoyasarTransactionResponse? TransactionUrl { get; set; }
    }

    public class MoyasarTransactionResponse
    {
        public string? Type { get; set; }
        public string? Value { get; set; }
    }

    /// <summary>
    /// Moyasar refund request
    /// Reference: https://docs.moyasar.com/api/payments/05-refund-payment
    /// </summary>
    public class MoyasarRefundRequest
    {
        public int Amount { get; set; }  // Amount in halalas
    }

    /// <summary>
    /// Moyasar error response
    /// </summary>
    public class MoyasarErrorResponse
    {
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public List<MoyasarError>? Errors { get; set; }
    }

    public class MoyasarError
    {
        public string Field { get; set; } = null!;
        public List<string> Messages { get; set; } = new();
    }

    /// <summary>
    /// Payment status constants from Moyasar
    /// </summary>
    public static class MoyasarPaymentStatus
    {
        public const string Initiated = "initiated";
        public const string Paid = "paid";
        public const string Failed = "failed";
        public const string Authorized = "authorized";
        public const string Captured = "captured";
        public const string Refunded = "refunded";
        public const string Voided = "voided";
    }

    /// <summary>
    /// Payment source types
    /// </summary>
    public static class MoyasarSourceType
    {
        public const string CreditCard = "creditcard";
        public const string ApplePay = "applepay";
        public const string STCPay = "stcpay";
        public const string Tabby = "tabby";
        public const string Tamara = "tamara";
    }
}

using System.Text.Json.Serialization;

namespace AutoPartsStore.Core.Models.Payments.Tap
{
    /// <summary>
    /// Tap payment charge request
    /// Reference: https://developers.tap.company/reference/create-a-charge
    /// </summary>
    public class TapCreateChargeRequest
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "SAR";

        [JsonPropertyName("threeDSecure")]
        public bool ThreeDSecure { get; set; } = true;

        [JsonPropertyName("save_card")]
        public bool SaveCard { get; set; } = false;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("statement_descriptor")]
        public string? StatementDescriptor { get; set; }

        [JsonPropertyName("metadata")]
        public TapMetadata? Metadata { get; set; }

        [JsonPropertyName("reference")]
        public TapReference? Reference { get; set; }

        [JsonPropertyName("receipt")]
        public TapReceipt? Receipt { get; set; }

        [JsonPropertyName("customer")]
        public TapCustomer Customer { get; set; } = null!;

        [JsonPropertyName("source")]
        public TapSource Source { get; set; } = null!;

        [JsonPropertyName("post")]
        public TapPost? Post { get; set; }

        [JsonPropertyName("redirect")]
        public TapRedirect Redirect { get; set; } = null!;
    }

    public class TapMetadata
    {
        [JsonPropertyName("udf1")]
        public string? OrderId { get; set; }

        [JsonPropertyName("udf2")]
        public string? OrderNumber { get; set; }

        [JsonPropertyName("udf3")]
        public string? UserId { get; set; }

        [JsonPropertyName("udf4")]
        public string? TransactionReference { get; set; }
    }

    public class TapReference
    {
        [JsonPropertyName("transaction")]
        public string? Transaction { get; set; }

        [JsonPropertyName("order")]
        public string? Order { get; set; }
    }

    public class TapReceipt
    {
        [JsonPropertyName("email")]
        public bool Email { get; set; } = false;

        [JsonPropertyName("sms")]
        public bool Sms { get; set; } = false;
    }

    public class TapCustomer
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = null!;

        [JsonPropertyName("middle_name")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = null!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("phone")]
        public TapPhone? Phone { get; set; }
    }

    public class TapPhone
    {
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = "966";

        [JsonPropertyName("number")]
        public string Number { get; set; } = null!;
    }

    public class TapSource
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
    }

    public class TapPost
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;
    }

    public class TapRedirect
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;
    }

    /// <summary>
    /// Tap charge response
    /// </summary>
    public class TapChargeResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("object")]
        public string Object { get; set; } = null!;

        [JsonPropertyName("live_mode")]
        public bool LiveMode { get; set; }

        [JsonPropertyName("api_version")]
        public string ApiVersion { get; set; } = null!;

        [JsonPropertyName("method")]
        public string Method { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;

        [JsonPropertyName("threeDSecure")]
        public bool ThreeDSecure { get; set; }

        [JsonPropertyName("save_card")]
        public bool SaveCard { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("statement_descriptor")]
        public string? StatementDescriptor { get; set; }

        [JsonPropertyName("metadata")]
        public TapMetadata? Metadata { get; set; }

        [JsonPropertyName("transaction")]
        public TapTransaction? Transaction { get; set; }

        [JsonPropertyName("reference")]
        public TapReference? Reference { get; set; }

        [JsonPropertyName("response")]
        public TapResponseDetails? Response { get; set; }

        [JsonPropertyName("receipt")]
        public TapReceiptResponse? Receipt { get; set; }

        [JsonPropertyName("customer")]
        public TapCustomerResponse? Customer { get; set; }

        [JsonPropertyName("source")]
        public TapSourceResponse? Source { get; set; }

        [JsonPropertyName("redirect")]
        public TapRedirectResponse? Redirect { get; set; }

        [JsonPropertyName("post")]
        public TapPost? Post { get; set; }

        [JsonPropertyName("activities")]
        public List<TapActivity>? Activities { get; set; }

        [JsonPropertyName("auto")]
        public TapAuto? Auto { get; set; }

        [JsonPropertyName("card")]
        public TapCard? Card { get; set; }
    }

    public class TapTransaction
    {
        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("expiry")]
        public TapExpiry? Expiry { get; set; }

        [JsonPropertyName("asynchronous")]
        public bool Asynchronous { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
    }

    public class TapExpiry
    {
        [JsonPropertyName("period")]
        public int Period { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class TapResponseDetails
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class TapReceiptResponse
    {
        [JsonPropertyName("email")]
        public bool Email { get; set; }

        [JsonPropertyName("sms")]
        public bool Sms { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class TapCustomerResponse
    {
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("middle_name")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phone")]
        public TapPhone? Phone { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class TapSourceResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("scheme")]
        public string? Scheme { get; set; }

        [JsonPropertyName("first_six")]
        public string? FirstSix { get; set; }

        [JsonPropertyName("last_four")]
        public string? LastFour { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("expiry_month")]
        public int? ExpiryMonth { get; set; }

        [JsonPropertyName("expiry_year")]
        public int? ExpiryYear { get; set; }

        [JsonPropertyName("funding")]
        public string? Funding { get; set; }

        [JsonPropertyName("fingerprint")]
        public string? Fingerprint { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }

    public class TapRedirectResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class TapActivity
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }
    }

    public class TapAuto
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("time")]
        public int Time { get; set; }
    }

    public class TapCard
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("first_six")]
        public string? FirstSix { get; set; }

        [JsonPropertyName("scheme")]
        public string? Scheme { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("last_four")]
        public string? LastFour { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("expiry_month")]
        public int ExpiryMonth { get; set; }

        [JsonPropertyName("expiry_year")]
        public int ExpiryYear { get; set; }

        [JsonPropertyName("funding")]
        public string? Funding { get; set; }
    }

    /// <summary>
    /// Tap refund request
    /// </summary>
    public class TapRefundRequest
    {
        [JsonPropertyName("charge_id")]
        public string ChargeId { get; set; } = null!;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "SAR";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("reference")]
        public TapReference? Reference { get; set; }

        [JsonPropertyName("metadata")]
        public TapMetadata? Metadata { get; set; }
    }

    /// <summary>
    /// Tap payment status constants
    /// </summary>
    public static class TapPaymentStatus
    {
        public const string Initiated = "INITIATED";
        public const string InProgress = "IN_PROGRESS";
        public const string Abandoned = "ABANDONED";
        public const string Cancelled = "CANCELLED";
        public const string Failed = "FAILED";
        public const string Declined = "DECLINED";
        public const string Restricted = "RESTRICTED";
        public const string Captured = "CAPTURED";
        public const string Void = "VOID";
        public const string Timedout = "TIMEDOUT";
        public const string Unknown = "UNKNOWN";
    }

    /// <summary>
    /// Tap payment source types (payment methods)
    /// </summary>
    public static class TapSourceType
    {
        public const string Token = "tok_";           // Card token from Tap.js
        public const string Card = "card_";           // Saved card
        public const string ApplePay = "applepay";    // Apple Pay
        public const string Mada = "mada";            // Mada cards
        public const string Tabby = "tabby";          // Tabby installments
    }

    /// <summary>
    /// Tap error response
    /// </summary>
    public class TapErrorResponse
    {
        [JsonPropertyName("errors")]
        public List<TapError>? Errors { get; set; }
    }

    public class TapError
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}

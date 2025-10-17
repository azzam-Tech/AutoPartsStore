namespace AutoPartsStore.Core.Models.Payments.Tap
{
    /// <summary>
    /// Tap payment gateway settings
    /// Reference: https://developers.tap.company/docs/authentication
    /// </summary>
    public class TapSettings
    {
        /// <summary>
        /// Secret API Key (starts with sk_test_ or sk_live_)
        /// Used for server-side API calls
        /// </summary>
        public string SecretKey { get; set; } = null!;

        /// <summary>
        /// Publishable API Key (starts with pk_test_ or pk_live_)
        /// Used for client-side (Tap.js)
        /// </summary>
        public string PublishableKey { get; set; } = null!;

        /// <summary>
        /// Tap API base URL
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.tap.company/v2";

        /// <summary>
        /// Webhook URL for payment notifications
        /// </summary>
        public string WebhookUrl { get; set; } = null!;

        /// <summary>
        /// Redirect URL after payment completion
        /// </summary>
        public string RedirectUrl { get; set; } = null!;

        /// <summary>
        /// Merchant ID (optional)
        /// </summary>
        public string? MerchantId { get; set; }

        /// <summary>
        /// Enable 3D Secure by default
        /// </summary>
        public bool Enable3DSecure { get; set; } = true;

        /// <summary>
        /// Save cards for future use
        /// </summary>
        public bool SaveCards { get; set; } = false;

        /// <summary>
        /// Statement descriptor (appears on customer's bank statement)
        /// </summary>
        public string? StatementDescriptor { get; set; }
    }
}

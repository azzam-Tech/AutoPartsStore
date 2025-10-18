using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    /// <summary>
    /// Validates Tap webhook signatures using HMAC-SHA256
    /// Reference: https://developers.tap.company/docs/webhook
    /// </summary>
    public class TapWebhookValidator
    {
        private readonly ILogger<TapWebhookValidator> _logger;

        public TapWebhookValidator(ILogger<TapWebhookValidator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates the webhook signature (hashstring)
        /// </summary>
        /// <param name="chargeId">Charge ID from webhook</param>
        /// <param name="amount">Amount (formatted with 2 decimal places)</param>
        /// <param name="currency">Currency (SAR)</param>
        /// <param name="gatewayRef">Gateway reference</param>
        /// <param name="paymentRef">Payment reference</param>
        /// <param name="status">Payment status</param>
        /// <param name="created">Transaction created timestamp (ms)</param>
        /// <param name="receivedHash">Hash received from Tap in header</param>
        /// <param name="secretKey">Tap Secret Key</param>
        /// <returns>True if signature is valid</returns>
        public bool ValidateSignature(
            string chargeId,
            decimal amount,
            string currency,
            string gatewayRef,
            string paymentRef,
            string status,
            string created,
            string receivedHash,
            string secretKey)
        {
            try
            {
                // Format amount with 2 decimal places (SAR) using InvariantCulture
                var amountFormatted = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

                // Build the string to be hashed (order matters!)
                // Format: x_id{id}x_amount{amount}x_currency{currency}x_gateway_reference{gateway_ref}
                //         x_payment_reference{payment_ref}x_status{status}x_created{created}
                var toBeHashed =
                    $"x_id{chargeId}" +
                    $"x_amount{amountFormatted}" +
                    $"x_currency{currency}" +
                    $"x_gateway_reference{gatewayRef}" +
                    $"x_payment_reference{paymentRef}" +
                    $"x_status{status}" +
                    $"x_created{created}";

                _logger.LogDebug("Hash string to verify: {HashString}", toBeHashed);

                // Compute HMAC-SHA256
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed));
                var computedHash = Convert.ToHexString(hash).ToLowerInvariant();

                _logger.LogDebug("Computed hash: {ComputedHash}", computedHash);
                _logger.LogDebug("Received hash: {ReceivedHash}", receivedHash);

                // Compare hashes (case-insensitive)
                var isValid = computedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);

                if (!isValid)
                {
                    _logger.LogWarning(
                        "Webhook signature validation failed. ChargeId: {ChargeId}, " +
                        "Expected: {Expected}, Received: {Received}",
                        chargeId, computedHash, receivedHash);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature for charge {ChargeId}", chargeId);
                return false;
            }
        }

        /// <summary>
        /// Validates webhook signature from raw JSON payload
        /// </summary>
        public bool ValidateFromJson(
            string jsonPayload,
            string receivedHash,
            string secretKey)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(jsonPayload);
                var root = doc.RootElement;

                var chargeId = root.GetProperty("id").GetString() ?? "";
                var amount = root.GetProperty("amount").GetDecimal();
                var currency = root.GetProperty("currency").GetString() ?? "";
                var status = root.GetProperty("status").GetString() ?? "";
                var created = root.GetProperty("transaction").GetProperty("created").GetRawText();

                var gatewayRef = root.GetProperty("reference").GetProperty("gateway").GetString() ?? "";
                var paymentRef = root.GetProperty("reference").GetProperty("payment").GetString() ?? "";

                return ValidateSignature(
                    chargeId,
                    amount,
                    currency,
                    gatewayRef,
                    paymentRef,
                    status,
                    created,
                    receivedHash,
                    secretKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing webhook JSON for signature validation");
                return false;
            }
        }
    }
}

using AutoPartsStore.Core.Exceptions;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Payments.Moyasar;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AutoPartsStore.Infrastructure.Services
{
    public class MoyasarService : IMoyasarService
    {
        private readonly HttpClient _httpClient;
        private readonly MoyasarSettings _settings;
        private readonly ILogger<MoyasarService> _logger;

        public MoyasarService(
            HttpClient httpClient,
            IOptions<MoyasarSettings> settings,
            ILogger<MoyasarService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            // Configure HTTP client
            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);

            // Basic Authentication: ApiKey as username, empty password
            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_settings.ApiKey}:"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authToken);

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<MoyasarPaymentResponse> CreatePaymentAsync(MoyasarCreatePaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating Moyasar payment for amount: {Amount} {Currency}",
                    request.Amount, request.Currency);

                // Convert amount to halalas (smallest unit)
                var paymentRequest = new
                {
                    amount = (int)(request.Amount * 100), // SAR to halalas
                    currency = request.Currency,
                    description = request.Description,
                    callback_url = request.CallbackUrl ?? _settings.CallbackUrl,
                    source = MapSource(request.Source),
                    metadata = request.Metadata
                };

                var response = await _httpClient.PostAsJsonAsync("/v1/payments", paymentRequest);

                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<MoyasarPaymentResponse>();
                    
                    _logger.LogInformation("Moyasar payment created successfully. PaymentId: {PaymentId}",
                        payment?.Id);

                    return payment!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Moyasar payment creation failed. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                throw new ExternalServiceException(
                    $"Failed to create payment with Moyasar. Status: {response.StatusCode}",
                    "Moyasar",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while creating Moyasar payment");
                throw new ExternalServiceException(
                    "Failed to connect to Moyasar payment gateway",
                    "Moyasar",
                    ex.Message,
                    innerException: ex);
            }
            catch (ExternalServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating Moyasar payment");
                throw new InternalServerException(
                    "An unexpected error occurred while processing payment",
                    innerException: ex);
            }
        }

        public async Task<MoyasarPaymentResponse> GetPaymentAsync(string paymentId)
        {
            try
            {
                _logger.LogInformation("Fetching Moyasar payment: {PaymentId}", paymentId);

                var response = await _httpClient.GetAsync($"/v1/payments/{paymentId}");

                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<MoyasarPaymentResponse>();
                    return payment!;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new NotFoundException(
                        $"Payment with ID '{paymentId}' not found in Moyasar",
                        "Payment",
                        paymentId);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to fetch Moyasar payment. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                throw new ExternalServiceException(
                    "Failed to fetch payment from Moyasar",
                    "Moyasar",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching Moyasar payment");
                throw new ExternalServiceException(
                    "Failed to connect to Moyasar payment gateway",
                    "Moyasar",
                    ex.Message,
                    innerException: ex);
            }
        }

        public async Task<MoyasarPaymentResponse> RefundPaymentAsync(string paymentId, MoyasarRefundRequest request)
        {
            try
            {
                _logger.LogInformation("Refunding Moyasar payment: {PaymentId}, Amount: {Amount}",
                    paymentId, request.Amount);

                var refundRequest = new
                {
                    amount = request.Amount // Already in halalas
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"/v1/payments/{paymentId}/refund",
                    refundRequest);

                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<MoyasarPaymentResponse>();
                    
                    _logger.LogInformation("Moyasar payment refunded successfully. PaymentId: {PaymentId}",
                        paymentId);

                    return payment!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Moyasar refund failed. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                throw new ExternalServiceException(
                    "Failed to refund payment with Moyasar",
                    "Moyasar",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while refunding Moyasar payment");
                throw new ExternalServiceException(
                    "Failed to connect to Moyasar payment gateway",
                    "Moyasar",
                    ex.Message,
                    innerException: ex);
            }
        }

        public async Task<MoyasarPaymentResponse> CapturePaymentAsync(string paymentId)
        {
            try
            {
                _logger.LogInformation("Capturing Moyasar payment: {PaymentId}", paymentId);

                var response = await _httpClient.PostAsync(
                    $"/v1/payments/{paymentId}/capture",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<MoyasarPaymentResponse>();
                    
                    _logger.LogInformation("Moyasar payment captured successfully. PaymentId: {PaymentId}",
                        paymentId);

                    return payment!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ExternalServiceException(
                    "Failed to capture payment with Moyasar",
                    "Moyasar",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalServiceException(
                    "Failed to connect to Moyasar payment gateway",
                    "Moyasar",
                    ex.Message,
                    innerException: ex);
            }
        }

        public async Task<MoyasarPaymentResponse> VoidPaymentAsync(string paymentId)
        {
            try
            {
                _logger.LogInformation("Voiding Moyasar payment: {PaymentId}", paymentId);

                var response = await _httpClient.PostAsync(
                    $"/v1/payments/{paymentId}/void",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<MoyasarPaymentResponse>();
                    
                    _logger.LogInformation("Moyasar payment voided successfully. PaymentId: {PaymentId}",
                        paymentId);

                    return payment!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ExternalServiceException(
                    "Failed to void payment with Moyasar",
                    "Moyasar",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalServiceException(
                    "Failed to connect to Moyasar payment gateway",
                    "Moyasar",
                    ex.Message,
                    innerException: ex);
            }
        }

        public async Task<MoyasarPaymentResponse> UpdatePaymentAsync(
            string paymentId,
            Dictionary<string, string> metadata)
        {
            try
            {
                _logger.LogInformation("Updating Moyasar payment metadata: {PaymentId}", paymentId);

                var updateRequest = new
                {
                    metadata = metadata
                };

                var response = await _httpClient.PutAsJsonAsync(
                    $"/v1/payments/{paymentId}",
                    updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<MoyasarPaymentResponse>();
                    return payment!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ExternalServiceException(
                    "Failed to update payment with Moyasar",
                    "Moyasar",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalServiceException(
                    "Failed to connect to Moyasar payment gateway",
                    "Moyasar",
                    ex.Message,
                    innerException: ex);
            }
        }

        public async Task<List<MoyasarPaymentResponse>> ListPaymentsAsync(int page = 1, int perPage = 20)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"/v1/payments?page={page}&per_page={perPage}");

                if (response.IsSuccessStatusCode)
                {
                    var payments = await response.Content.ReadFromJsonAsync<List<MoyasarPaymentResponse>>();
                    return payments ?? new List<MoyasarPaymentResponse>();
                }

                return new List<MoyasarPaymentResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while listing Moyasar payments");
                return new List<MoyasarPaymentResponse>();
            }
        }

        private object MapSource(MoyasarSource source)
        {
            var mappedSource = new Dictionary<string, string>
            {
                ["type"] = source.Type
            };

            // Add fields based on source type
            if (source.Type == MoyasarSourceType.CreditCard)
            {
                if (!string.IsNullOrEmpty(source.Number))
                    mappedSource["number"] = source.Number;
                if (!string.IsNullOrEmpty(source.Name))
                    mappedSource["name"] = source.Name;
                if (!string.IsNullOrEmpty(source.Month))
                    mappedSource["month"] = source.Month;
                if (!string.IsNullOrEmpty(source.Year))
                    mappedSource["year"] = source.Year;
                if (!string.IsNullOrEmpty(source.Cvc))
                    mappedSource["cvc"] = source.Cvc;
            }
            else if (source.Type == MoyasarSourceType.ApplePay && !string.IsNullOrEmpty(source.Token))
            {
                mappedSource["token"] = source.Token;
            }
            else if ((source.Type == MoyasarSourceType.Tabby || source.Type == MoyasarSourceType.Tamara) 
                     && !string.IsNullOrEmpty(source.Company))
            {
                mappedSource["company"] = source.Company;
            }

            return mappedSource;
        }
    }
}

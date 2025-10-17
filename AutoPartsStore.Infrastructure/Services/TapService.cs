using AutoPartsStore.Core.Exceptions;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Payments.Tap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AutoPartsStore.Infrastructure.Services
{
    /// <summary>
    /// Tap payment gateway service implementation
    /// Reference: https://developers.tap.company/reference/api-endpoint
    /// </summary>
    public class TapService : ITapService
    {
        private readonly HttpClient _httpClient;
        private readonly TapSettings _settings;
        private readonly ILogger<TapService> _logger;

        public TapService(
            HttpClient httpClient,
            IOptions<TapSettings> settings,
            ILogger<TapService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);

            // Bearer Token authentication
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.SecretKey);

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<TapChargeResponse> CreateChargeAsync(TapCreateChargeRequest request)
        {
            try
            {
                _logger.LogInformation("Creating Tap charge for amount: {Amount} {Currency}",
                    request.Amount, request.Currency);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                var response = await _httpClient.PostAsJsonAsync("/charges", request, options);

                if (response.IsSuccessStatusCode)
                {
                    var charge = await response.Content.ReadFromJsonAsync<TapChargeResponse>(options);

                    _logger.LogInformation("Tap charge created successfully. ChargeId: {ChargeId}, Status: {Status}",
                        charge?.Id, charge?.Status);

                    return charge!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Tap charge creation failed. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                // Try to parse error response
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<TapErrorResponse>(errorContent, options);
                    var errorMessage = errorResponse?.Errors?.FirstOrDefault()?.Description
                                      ?? "Failed to create charge with Tap";

                    throw new ExternalServiceException(
                        errorMessage,
                        "Tap",
                        errorResponse?.Errors?.FirstOrDefault()?.Code);
                }
                catch (JsonException)
                {
                    throw new ExternalServiceException(
                        $"Failed to create charge with Tap. Status: {response.StatusCode}",
                        "Tap",
                        errorContent);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while creating Tap charge");
                throw new ExternalServiceException(
                    "Failed to connect to Tap payment gateway",
                    "Tap",
                    ex.Message,
                    innerException: ex);
            }
            catch (ExternalServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating Tap charge");
                throw new InternalServerException(
                    "An unexpected error occurred while processing payment",
                    innerException: ex);
            }
        }

        public async Task<TapChargeResponse> GetChargeAsync(string chargeId)
        {
            try
            {
                _logger.LogInformation("Fetching Tap charge: {ChargeId}", chargeId);

                var response = await _httpClient.GetAsync($"/charges/{chargeId}");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var charge = await response.Content.ReadFromJsonAsync<TapChargeResponse>(options);
                    return charge!;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new NotFoundException(
                        $"Charge with ID '{chargeId}' not found in Tap",
                        "Charge",
                        chargeId);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to fetch Tap charge. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                throw new ExternalServiceException(
                    "Failed to fetch charge from Tap",
                    "Tap",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching Tap charge");
                throw new ExternalServiceException(
                    "Failed to connect to Tap payment gateway",
                    "Tap",
                    ex.Message,
                    innerException: ex);
            }
        }

        public async Task<List<TapChargeResponse>> ListChargesAsync(int page = 1, int limit = 20)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"/charges?page={page}&limit={limit}");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var result = await response.Content.ReadFromJsonAsync<TapChargesListResponse>(options);
                    return result?.Charges ?? new List<TapChargeResponse>();
                }

                return new List<TapChargeResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while listing Tap charges");
                return new List<TapChargeResponse>();
            }
        }

        public async Task<TapChargeResponse> RefundChargeAsync(TapRefundRequest request)
        {
            try
            {
                _logger.LogInformation("Refunding Tap charge: {ChargeId}, Amount: {Amount}",
                    request.ChargeId, request.Amount);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                var response = await _httpClient.PostAsJsonAsync("/refunds", request, options);

                if (response.IsSuccessStatusCode)
                {
                    var charge = await response.Content.ReadFromJsonAsync<TapChargeResponse>(options);

                    _logger.LogInformation("Tap charge refunded successfully. ChargeId: {ChargeId}",
                        request.ChargeId);

                    return charge!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Tap refund failed. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                throw new ExternalServiceException(
                    "Failed to refund charge with Tap",
                    "Tap",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while refunding Tap charge");
                throw new ExternalServiceException(
                    "Failed to connect to Tap payment gateway",
                    "Tap",
                    ex.Message,
                    innerException: ex);
            }
        }

        public async Task<TapChargeResponse> VoidChargeAsync(string chargeId)
        {
            try
            {
                _logger.LogInformation("Voiding Tap charge: {ChargeId}", chargeId);

                var response = await _httpClient.PostAsync(
                    $"/charges/{chargeId}/void",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var charge = await response.Content.ReadFromJsonAsync<TapChargeResponse>(options);

                    _logger.LogInformation("Tap charge voided successfully. ChargeId: {ChargeId}",
                        chargeId);

                    return charge!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ExternalServiceException(
                    "Failed to void charge with Tap",
                    "Tap",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalServiceException(
                    "Failed to connect to Tap payment gateway",
                    "Tap",
                    ex.Message,
                    innerException: ex);
            }
        }

        public async Task<TapChargeResponse> UpdateChargeAsync(string chargeId, TapMetadata metadata)
        {
            try
            {
                _logger.LogInformation("Updating Tap charge metadata: {ChargeId}", chargeId);

                var updateRequest = new
                {
                    metadata = metadata
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                var response = await _httpClient.PutAsJsonAsync(
                    $"/charges/{chargeId}",
                    updateRequest,
                    options);

                if (response.IsSuccessStatusCode)
                {
                    var charge = await response.Content.ReadFromJsonAsync<TapChargeResponse>(options);
                    return charge!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ExternalServiceException(
                    "Failed to update charge with Tap",
                    "Tap",
                    errorContent);
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalServiceException(
                    "Failed to connect to Tap payment gateway",
                    "Tap",
                    ex.Message,
                    innerException: ex);
            }
        }
    }

    // Helper class for list response
    internal class TapChargesListResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("charges")]
        public List<TapChargeResponse>? Charges { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("has_more")]
        public bool HasMore { get; set; }
    }
}

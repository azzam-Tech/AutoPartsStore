using AutoPartsStore.Core.Models.Payments.Tap;

namespace AutoPartsStore.Core.Interfaces.IServices
{
    /// <summary>
    /// Tap payment gateway service interface
    /// </summary>
    public interface ITapService
    {
        /// <summary>
        /// Create a new charge/payment
        /// </summary>
        Task<TapChargeResponse> CreateChargeAsync(TapCreateChargeRequest request);

        /// <summary>
        /// Retrieve a charge by ID
        /// </summary>
        Task<TapChargeResponse> GetChargeAsync(string chargeId);

        /// <summary>
        /// List all charges (with pagination)
        /// </summary>
        Task<List<TapChargeResponse>> ListChargesAsync(int page = 1, int limit = 20);

        /// <summary>
        /// Refund a charge (full or partial)
        /// </summary>
        Task<TapChargeResponse> RefundChargeAsync(TapRefundRequest request);

        /// <summary>
        /// Void/Cancel an authorized charge
        /// </summary>
        Task<TapChargeResponse> VoidChargeAsync(string chargeId);

        /// <summary>
        /// Update charge metadata
        /// </summary>
        Task<TapChargeResponse> UpdateChargeAsync(string chargeId, TapMetadata metadata);
    }
}

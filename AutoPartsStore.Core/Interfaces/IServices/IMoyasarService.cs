using AutoPartsStore.Core.Models.Payments.Moyasar;

namespace AutoPartsStore.Core.Interfaces.IServices
{
    /// <summary>
    /// Moyasar payment gateway integration service
    /// </summary>
    public interface IMoyasarService
    {
        /// <summary>
        /// Create a new payment
        /// </summary>
        Task<MoyasarPaymentResponse> CreatePaymentAsync(MoyasarCreatePaymentRequest request);
        
        /// <summary>
        /// Fetch payment details
        /// </summary>
        Task<MoyasarPaymentResponse> GetPaymentAsync(string paymentId);
        
        /// <summary>
        /// Refund a payment
        /// </summary>
        Task<MoyasarPaymentResponse> RefundPaymentAsync(string paymentId, MoyasarRefundRequest request);
        
        /// <summary>
        /// Capture an authorized payment
        /// </summary>
        Task<MoyasarPaymentResponse> CapturePaymentAsync(string paymentId);
        
        /// <summary>
        /// Void a payment
        /// </summary>
        Task<MoyasarPaymentResponse> VoidPaymentAsync(string paymentId);
        
        /// <summary>
        /// Update payment metadata
        /// </summary>
        Task<MoyasarPaymentResponse> UpdatePaymentAsync(string paymentId, Dictionary<string, string> metadata);
        
        /// <summary>
        /// List payments with pagination
        /// </summary>
        Task<List<MoyasarPaymentResponse>> ListPaymentsAsync(int page = 1, int perPage = 20);
    }
}

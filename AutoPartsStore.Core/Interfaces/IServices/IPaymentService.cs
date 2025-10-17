using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Payments;
using AutoPartsStore.Core.Models.Payments.Tap;

namespace AutoPartsStore.Core.Interfaces.IServices
{
    public interface IPaymentService
    {
        // Payment initiation - Updated to return TapChargeResponse
        Task<TapChargeResponse> InitiatePaymentAsync(InitiatePaymentRequest request);
        
        // Payment processing - Updated webhook method
        Task<PaymentTransactionDto> ProcessPaymentWebhookAsync(string chargeId);
        Task<PaymentTransactionDto> VerifyPaymentAsync(string chargeId);
        
        // Payment queries
        Task<PaymentTransactionDto?> GetPaymentByIdAsync(int id);
        Task<PaymentTransactionDto?> GetPaymentByOrderIdAsync(int orderId);
        Task<PagedResult<PaymentTransactionDto>> GetPaymentsAsync(PaymentFilterRequest filter);
        Task<List<PaymentTransactionDto>> GetUserPaymentsAsync(int userId);
        
        // Refunds
        Task<PaymentTransactionDto> RefundPaymentAsync(int paymentId, RefundPaymentRequest request);
        
        // Statistics
        Task<PaymentSummaryDto> GetPaymentSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}

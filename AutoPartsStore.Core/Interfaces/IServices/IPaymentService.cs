using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Payments;
using AutoPartsStore.Core.Models.Payments.Moyasar;

namespace AutoPartsStore.Core.Interfaces.IServices
{
    public interface IPaymentService
    {
        // Payment initiation
        Task<MoyasarPaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request);
        
        // Payment processing
        Task<PaymentTransactionDto> ProcessPaymentCallbackAsync(string paymentId);
        Task<PaymentTransactionDto> VerifyPaymentAsync(string paymentId);
        
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

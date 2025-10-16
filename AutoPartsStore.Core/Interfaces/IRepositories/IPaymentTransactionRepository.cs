using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Payments;

namespace AutoPartsStore.Core.Interfaces.IRepositories
{
    public interface IPaymentTransactionRepository : IBaseRepository<PaymentTransaction>
    {
        Task<PaymentTransactionDto?> GetByIdWithDetailsAsync(int id);
        Task<PaymentTransactionDto?> GetByMoyasarPaymentIdAsync(string moyasarPaymentId);
        Task<PaymentTransactionDto?> GetByTransactionReferenceAsync(string reference);
        Task<PaymentTransactionDto?> GetByOrderIdAsync(int orderId);
        Task<PagedResult<PaymentTransactionDto>> GetFilteredTransactionsAsync(PaymentFilterRequest filter);
        Task<List<PaymentTransactionDto>> GetUserTransactionsAsync(int userId);
        Task<PaymentSummaryDto> GetPaymentSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}

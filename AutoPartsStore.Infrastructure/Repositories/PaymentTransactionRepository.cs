using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Payments;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : BaseRepository<PaymentTransaction>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(AppDbContext context) : base(context) { }

        public async Task<PaymentTransactionDto?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.PaymentTransactions
                .Where(pt => pt.Id == id)
                .Select(pt => new PaymentTransactionDto
                {
                    Id = pt.Id,
                    OrderId = pt.OrderId,
                    OrderNumber = pt.Order.OrderNumber,
                    UserId = pt.UserId,
                    UserName = pt.User.FullName,
                    TapChargeId = pt.TapChargeId,  // Updated from Moyasar
                    TransactionReference = pt.TransactionReference,
                    PaymentMethod = pt.PaymentMethod,
                    PaymentMethodText = pt.PaymentMethod.ToString(),
                    Status = pt.Status,
                    StatusText = pt.Status.ToString(),
                    Amount = pt.Amount,
                    Currency = pt.Currency,
                    AuthorizationCode = pt.AuthorizationCode,
                    ErrorMessage = pt.ErrorMessage,
                    ErrorCode = pt.ErrorCode,
                    CardLast4 = pt.CardLast4,
                    CardBrand = pt.CardBrand,
                    CardScheme = pt.CardScheme,
                    RefundedAmount = pt.RefundedAmount,
                    RefundedDate = pt.RefundedDate,
                    RefundReason = pt.RefundReason,
                    InitiatedDate = pt.InitiatedDate,
                    CompletedDate = pt.CompletedDate,
                    FailedDate = pt.FailedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTransactionDto?> GetByTapChargeIdAsync(string tapChargeId)
        {
            return await _context.PaymentTransactions
                .Where(pt => pt.TapChargeId == tapChargeId)
                .Select(pt => new PaymentTransactionDto
                {
                    Id = pt.Id,
                    OrderId = pt.OrderId,
                    OrderNumber = pt.Order.OrderNumber,
                    UserId = pt.UserId,
                    UserName = pt.User.FullName,
                    TapChargeId = pt.TapChargeId,
                    TransactionReference = pt.TransactionReference,
                    PaymentMethod = pt.PaymentMethod,
                    PaymentMethodText = pt.PaymentMethod.ToString(),
                    Status = pt.Status,
                    StatusText = pt.Status.ToString(),
                    Amount = pt.Amount,
                    Currency = pt.Currency,
                    AuthorizationCode = pt.AuthorizationCode,
                    ErrorMessage = pt.ErrorMessage,
                    ErrorCode = pt.ErrorCode,
                    CardLast4 = pt.CardLast4,
                    CardBrand = pt.CardBrand,
                    CardScheme = pt.CardScheme,
                    RefundedAmount = pt.RefundedAmount,
                    RefundedDate = pt.RefundedDate,
                    RefundReason = pt.RefundReason,
                    InitiatedDate = pt.InitiatedDate,
                    CompletedDate = pt.CompletedDate,
                    FailedDate = pt.FailedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTransactionDto?> GetByTransactionReferenceAsync(string reference)
        {
            return await _context.PaymentTransactions
                .Where(pt => pt.TransactionReference == reference)
                .Select(pt => new PaymentTransactionDto
                {
                    Id = pt.Id,
                    OrderId = pt.OrderId,
                    OrderNumber = pt.Order.OrderNumber,
                    UserId = pt.UserId,
                    UserName = pt.User.FullName,
                    TapChargeId = pt.TapChargeId,
                    TransactionReference = pt.TransactionReference,
                    PaymentMethod = pt.PaymentMethod,
                    PaymentMethodText = pt.PaymentMethod.ToString(),
                    Status = pt.Status,
                    StatusText = pt.Status.ToString(),
                    Amount = pt.Amount,
                    Currency = pt.Currency,
                    InitiatedDate = pt.InitiatedDate,
                    CompletedDate = pt.CompletedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTransactionDto?> GetByOrderIdAsync(int orderId)
        {
            return await _context.PaymentTransactions
                .Where(pt => pt.OrderId == orderId)
                .Select(pt => new PaymentTransactionDto
                {
                    Id = pt.Id,
                    OrderId = pt.OrderId,
                    OrderNumber = pt.Order.OrderNumber,
                    UserId = pt.UserId,
                    UserName = pt.User.FullName,
                    TapChargeId = pt.TapChargeId,
                    TransactionReference = pt.TransactionReference,
                    PaymentMethod = pt.PaymentMethod,
                    PaymentMethodText = pt.PaymentMethod.ToString(),
                    Status = pt.Status,
                    StatusText = pt.Status.ToString(),
                    Amount = pt.Amount,
                    Currency = pt.Currency,
                    AuthorizationCode = pt.AuthorizationCode,
                    ErrorMessage = pt.ErrorMessage,
                    CardLast4 = pt.CardLast4,
                    CardBrand = pt.CardBrand,
                    CardScheme = pt.CardScheme,
                    RefundedAmount = pt.RefundedAmount,
                    RefundedDate = pt.RefundedDate,
                    InitiatedDate = pt.InitiatedDate,
                    CompletedDate = pt.CompletedDate,
                    FailedDate = pt.FailedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<PaymentTransactionDto>> GetFilteredTransactionsAsync(PaymentFilterRequest filter)
        {
            var query = _context.PaymentTransactions.AsQueryable();

            // Apply filters
            if (filter.UserId.HasValue)
                query = query.Where(pt => pt.UserId == filter.UserId.Value);

            if (filter.OrderId.HasValue)
                query = query.Where(pt => pt.OrderId == filter.OrderId.Value);

            if (filter.Status.HasValue)
                query = query.Where(pt => pt.Status == filter.Status.Value);

            if (filter.PaymentMethod.HasValue)
                query = query.Where(pt => pt.PaymentMethod == filter.PaymentMethod.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(pt => pt.InitiatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(pt => pt.InitiatedDate <= filter.ToDate.Value);

            var totalCount = await query.CountAsync();

            var payments = await query
                .OrderByDescending(pt => pt.InitiatedDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(pt => new PaymentTransactionDto
                {
                    Id = pt.Id,
                    OrderId = pt.OrderId,
                    OrderNumber = pt.Order.OrderNumber,
                    UserId = pt.UserId,
                    UserName = pt.User.FullName,
                    TapChargeId = pt.TapChargeId,
                    TransactionReference = pt.TransactionReference,
                    PaymentMethod = pt.PaymentMethod,
                    PaymentMethodText = pt.PaymentMethod.ToString(),
                    Status = pt.Status,
                    StatusText = pt.Status.ToString(),
                    Amount = pt.Amount,
                    Currency = pt.Currency,
                    CardLast4 = pt.CardLast4,
                    CardBrand = pt.CardBrand,
                    CardScheme = pt.CardScheme,
                    InitiatedDate = pt.InitiatedDate,
                    CompletedDate = pt.CompletedDate,
                    FailedDate = pt.FailedDate
                })
                .ToListAsync();

            return new PagedResult<PaymentTransactionDto>
            {
                Items = payments,
                TotalCount = totalCount,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<List<PaymentTransactionDto>> GetUserTransactionsAsync(int userId)
        {
            return await _context.PaymentTransactions
                .Where(pt => pt.UserId == userId)
                .OrderByDescending(pt => pt.InitiatedDate)
                .Select(pt => new PaymentTransactionDto
                {
                    Id = pt.Id,
                    OrderId = pt.OrderId,
                    OrderNumber = pt.Order.OrderNumber,
                    UserId = pt.UserId,
                    UserName = pt.User.FullName,
                    TransactionReference = pt.TransactionReference,
                    PaymentMethod = pt.PaymentMethod,
                    PaymentMethodText = pt.PaymentMethod.ToString(),
                    Status = pt.Status,
                    StatusText = pt.Status.ToString(),
                    Amount = pt.Amount,
                    Currency = pt.Currency,
                    InitiatedDate = pt.InitiatedDate,
                    CompletedDate = pt.CompletedDate
                })
                .ToListAsync();
        }

        public async Task<PaymentSummaryDto> GetPaymentSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.PaymentTransactions.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(pt => pt.InitiatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(pt => pt.InitiatedDate <= toDate.Value);

            var transactions = await query.ToListAsync();

            return new PaymentSummaryDto
            {
                TotalTransactions = transactions.Count,
                TotalAmount = transactions.Where(pt => pt.Status == PaymentStatus.Paid || pt.Status == PaymentStatus.Captured).Sum(pt => pt.Amount),
                SuccessfulPayments = transactions.Count(pt => pt.Status == PaymentStatus.Paid || pt.Status == PaymentStatus.Captured),
                FailedPayments = transactions.Count(pt => pt.Status == PaymentStatus.Failed),
                RefundedPayments = transactions.Count(pt => pt.Status == PaymentStatus.Refunded || pt.Status == PaymentStatus.PartiallyRefunded),
                TotalRefunded = transactions.Where(pt => pt.RefundedAmount.HasValue).Sum(pt => pt.RefundedAmount ?? 0)
            };
        }
    }
}

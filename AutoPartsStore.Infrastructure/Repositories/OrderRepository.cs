using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Orders;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public async Task<OrderDto?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Where(o => o.Id == id && !o.IsDeleted)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    UserId = o.UserId,
                    UserName = o.User.FullName,
                    UserEmail = o.User.Email,
                    ShippingAddressId = o.ShippingAddressId,
                    ShippingAddress = $"{o.ShippingAddress.StreetName} {o.ShippingAddress.StreetNumber}, {o.ShippingAddress.District.DistrictName}, {o.ShippingAddress.District.City.CityName}",
                    SubTotal = o.SubTotal,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    StatusText = o.Status.ToString(),
                    OrderDate = o.OrderDate,
                    PaidDate = o.PaidDate,
                    ShippedDate = o.ShippedDate,
                    DeliveredDate = o.DeliveredDate,
                    CancelledDate = o.CancelledDate,
                    CancellationReason = o.CancellationReason,
                    PaymentTransactionId = o.PaymentTransactionId,
                    PaymentStatus = o.PaymentTransaction != null ? o.PaymentTransaction.Status.ToString() : null,
                    PaymentMethod = o.PaymentTransaction != null ? o.PaymentTransaction.PaymentMethod.ToString() : null,
                    CustomerNotes = o.CustomerNotes,
                    AdminNotes = o.AdminNotes,
                    TrackingNumber = o.TrackingNumber,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        PartId = oi.PartId,
                        PartNumber = oi.PartNumber,
                        PartName = oi.PartName,
                        ImageUrl = oi.ImageUrl,
                        UnitPrice = oi.UnitPrice,
                        DiscountPercent = oi.DiscountPercent,
                        Quantity = oi.Quantity,
                        PromotionId = oi.PromotionId,
                        PromotionName = oi.PromotionName,
                        PromotionDiscountType = oi.PromotionDiscountType.HasValue ? oi.PromotionDiscountType.ToString() : null,
                        PromotionDiscountValue = oi.PromotionDiscountValue,
                        SubTotal = oi.SubTotal,
                        DiscountAmount = oi.DiscountAmount,
                        FinalPrice = oi.FinalPrice,
                        TotalAmount = oi.TotalAmount
                    }).ToList(),
                    TotalItems = o.OrderItems.Count,
                    TotalQuantity = o.OrderItems.Sum(oi => oi.Quantity)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Where(o => o.OrderNumber == orderNumber && !o.IsDeleted)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    UserId = o.UserId,
                    UserName = o.User.FullName,
                    UserEmail = o.User.Email,
                    ShippingAddressId = o.ShippingAddressId,
                    ShippingAddress = $"{o.ShippingAddress.StreetName} {o.ShippingAddress.StreetNumber}, {o.ShippingAddress.District.DistrictName}, {o.ShippingAddress.District.City.CityName}",
                    SubTotal = o.SubTotal,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    StatusText = o.Status.ToString(),
                    OrderDate = o.OrderDate,
                    PaidDate = o.PaidDate,
                    ShippedDate = o.ShippedDate,
                    DeliveredDate = o.DeliveredDate,
                    CancelledDate = o.CancelledDate,
                    CancellationReason = o.CancellationReason,
                    PaymentTransactionId = o.PaymentTransactionId,
                    PaymentStatus = o.PaymentTransaction != null ? o.PaymentTransaction.Status.ToString() : null,
                    PaymentMethod = o.PaymentTransaction != null ? o.PaymentTransaction.PaymentMethod.ToString() : null,
                    CustomerNotes = o.CustomerNotes,
                    AdminNotes = o.AdminNotes,
                    TrackingNumber = o.TrackingNumber,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        PartId = oi.PartId,
                        PartNumber = oi.PartNumber,
                        PartName = oi.PartName,
                        ImageUrl = oi.ImageUrl,
                        UnitPrice = oi.UnitPrice,
                        DiscountPercent = oi.DiscountPercent,
                        Quantity = oi.Quantity,
                        PromotionId = oi.PromotionId,
                        PromotionName = oi.PromotionName,
                        PromotionDiscountType = oi.PromotionDiscountType.HasValue ? oi.PromotionDiscountType.ToString() : null,
                        PromotionDiscountValue = oi.PromotionDiscountValue,
                        SubTotal = oi.SubTotal,
                        DiscountAmount = oi.DiscountAmount,
                        FinalPrice = oi.FinalPrice,
                        TotalAmount = oi.TotalAmount
                    }).ToList(),
                    TotalItems = o.OrderItems.Count,
                    TotalQuantity = o.OrderItems.Sum(oi => oi.Quantity)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<OrderDto>> GetFilteredOrdersAsync(OrderFilterRequest filter)
        {
            var query = _context.Orders
                .Where(o => !o.IsDeleted)
                .AsQueryable();

            // Apply filters
            if (filter.UserId.HasValue)
                query = query.Where(o => o.UserId == filter.UserId.Value);

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == (OrderStatus)filter.Status.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.OrderDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.OrderDate <= filter.ToDate.Value);

            if (filter.MinAmount.HasValue)
                query = query.Where(o => o.TotalAmount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(o => o.TotalAmount <= filter.MaxAmount.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(o =>
                    o.OrderNumber.Contains(filter.SearchTerm) ||
                    o.User.FullName.Contains(filter.SearchTerm) ||
                    o.User.Email.Contains(filter.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    UserId = o.UserId,
                    UserName = o.User.FullName,
                    UserEmail = o.User.Email,
                    ShippingAddressId = o.ShippingAddressId,
                    ShippingAddress = $"{o.ShippingAddress.District.City.CityName}",
                    SubTotal = o.SubTotal,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    StatusText = o.Status.ToString(),
                    OrderDate = o.OrderDate,
                    PaidDate = o.PaidDate,
                    PaymentTransactionId = o.PaymentTransactionId,
                    TotalItems = o.OrderItems.Count,
                    TotalQuantity = o.OrderItems.Sum(oi => oi.Quantity)
                })
                .ToListAsync();

            return new PagedResult<OrderDto>
            {
                Items = orders,
                TotalCount = totalCount,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    StatusText = o.Status.ToString(),
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    TotalItems = o.OrderItems.Count
                })
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }

        public async Task<bool> OrderNumberExistsAsync(string orderNumber)
        {
            return await _context.Orders
                .AnyAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Orders
                .Where(o => !o.IsDeleted && o.Status == OrderStatus.Delivered);

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersCountAsync(OrderStatus? status = null)
        {
            var query = _context.Orders.Where(o => !o.IsDeleted);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            return await query.CountAsync();
        }

        public async Task<List<OrderDto>> GetRecentOrdersAsync(int count = 10)
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    UserId = o.UserId,
                    UserName = o.User.FullName,
                    UserEmail = o.User.Email,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    StatusText = o.Status.ToString(),
                    OrderDate = o.OrderDate,
                    TotalItems = o.OrderItems.Count
                })
                .ToListAsync();
        }
    }
}

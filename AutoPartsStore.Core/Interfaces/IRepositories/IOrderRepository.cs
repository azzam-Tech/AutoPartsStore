using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Orders;

namespace AutoPartsStore.Core.Interfaces.IRepositories
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<OrderDto?> GetByIdWithDetailsAsync(int id);
        Task<OrderDto?> GetByOrderNumberAsync(string orderNumber);
        Task<PagedResult<OrderDto>> GetFilteredOrdersAsync(OrderFilterRequest filter);
        Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<bool> OrderNumberExistsAsync(string orderNumber);
        Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetTotalOrdersCountAsync(OrderStatus? status = null);
        Task<List<OrderDto>> GetRecentOrdersAsync(int count = 10);
    }
}

using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Orders;

namespace AutoPartsStore.Core.Interfaces.IServices
{
    public interface IOrderService
    {
        // Order CRUD
        Task<OrderDto> CreateOrderFromCartAsync(int userId, CreateOrderFromCartRequest request);
        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
        Task<PagedResult<OrderDto>> GetOrdersAsync(OrderFilterRequest filter);
        Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId);
        Task<List<OrderDto>> GetRecentOrdersAsync(int count = 10);
        
        // Order management
        Task<OrderDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request);
        Task<OrderDto> CancelOrderAsync(int orderId, int userId, CancelOrderRequest request);
        Task<OrderDto> UpdateTrackingNumberAsync(int orderId, UpdateTrackingRequest request);
        Task<bool> DeleteOrderAsync(int orderId);
        
        // Order calculations
        Task<decimal> CalculateOrderTotalAsync(List<CreateOrderItemRequest> items);
        
        // Statistics
        Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetTotalOrdersCountAsync(int? status = null);
        
        // Stock management
        Task ReduceStockAsync(Order order);
    }
}

using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Exceptions;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Orders;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShoppingCartRepository _cartRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IShoppingCartRepository cartRepository,
            IAddressRepository addressRepository,
            AppDbContext context,
            ILogger<OrderService> _logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
            _context = context;
            this._logger = _logger;
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(int userId, CreateOrderFromCartRequest request)
        {
            _logger.LogInformation("Creating order from cart for user {UserId}", userId);

            // Get user's cart
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any())
            {
                throw new BusinessException("”·… «· ”Êﬁ ›«—€…. ·« Ì„ﬂ‰ ≈‰‘«¡ ÿ·».");
            }

            // Validate address
            var address = await _addressRepository.GetByIdWithDetailsAsync(request.ShippingAddressId);
            if (address == null)
            {
                throw new NotFoundException("«·⁄‰Ê«‰ €Ì— „ÊÃÊœ.", "Address", request.ShippingAddressId);
            }

            if (address.UserId != userId)
            {
                throw new ForbiddenException("·« Ì„ﬂ‰ﬂ «” Œœ«„ ⁄‰Ê«‰ ·« ÌŒ’ﬂ.");
            }

            // Validate stock availability
            await ValidateStockAvailabilityAsync(cart.Items.Select(i => new CreateOrderItemRequest
            {
                PartId = i.PartId,
                Quantity = i.Quantity
            }).ToList());

            // Create order with placeholder values (will be recalculated from OrderItems)
            var order = new Order(
                userId,
                request.ShippingAddressId,
                0, // SubTotal - will be recalculated
                0, // DiscountAmount - will be recalculated
                request.CustomerNotes
            );

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Create order items from cart
            // OrderItem constructor automatically calculates prices using the BEST discount logic
            foreach (var cartItem in cart.Items)
            {
                var part = await _context.CarParts
                    .Include(p => p.Promotion)
                    .FirstOrDefaultAsync(p => p.Id == cartItem.PartId);

                if (part == null) continue;

                var orderItem = new OrderItem(
                    order.Id,
                    part.Id,
                    part.PartNumber,
                    part.PartName,
                    part.UnitPrice,
                    part.DiscountPercent,
                    cartItem.Quantity,
                    part.ImageUrl,
                    part.PromotionId,
                    part.Promotion?.PromotionName,
                    part.Promotion?.DiscountType,
                    part.Promotion?.DiscountValue
                );

                await _context.OrderItems.AddAsync(orderItem);
            }

            await _context.SaveChangesAsync();

            // IMPORTANT: Recalculate order totals from actual order items
            // OrderItem.CalculateAmounts() uses Math.Min to get the BEST price (product discount vs promotion)
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            decimal actualSubTotal = orderItems.Sum(oi => oi.SubTotal);
            decimal actualDiscount = orderItems.Sum(oi => oi.DiscountAmount);

            order.UpdateTotals(actualSubTotal, actualDiscount);

            await _context.SaveChangesAsync();

            // Clear the cart
            await ClearUserCartAsync(userId);

            _logger.LogInformation("Order {OrderNumber} created successfully for user {UserId}. Total: {Total} SAR",
                order.OrderNumber, userId, order.TotalAmount);

            // Return order details
            return (await _orderRepository.GetByIdWithDetailsAsync(order.Id))!;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _orderRepository.GetByOrderNumberAsync(orderNumber);
        }

        public async Task<PagedResult<OrderDto>> GetOrdersAsync(OrderFilterRequest filter)
        {
            return await _orderRepository.GetFilteredOrdersAsync(filter);
        }

        public async Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId)
        {
            return await _orderRepository.GetUserOrdersAsync(userId);
        }

        public async Task<List<OrderDto>> GetRecentOrdersAsync(int count = 10)
        {
            return await _orderRepository.GetRecentOrdersAsync(count);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("«·ÿ·» €Ì— „ÊÃÊœ.", "Order", orderId);
            }

            var newStatus = (OrderStatus)request.OrderStatus;
            
            _logger.LogInformation("Updating order {OrderNumber} status from {OldStatus} to {NewStatus}",
                order.OrderNumber, order.Status, newStatus);

            order.UpdateStatus(newStatus);

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                order.AddAdminNotes(request.Notes);
            }

            await _context.SaveChangesAsync();

            return (await _orderRepository.GetByIdWithDetailsAsync(orderId))!;
        }

        public async Task<OrderDto> CancelOrderAsync(int orderId, int userId, CancelOrderRequest request)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("«·ÿ·» €Ì— „ÊÃÊœ.", "Order", orderId);
            }

            if (order.UserId != userId)
            {
                throw new ForbiddenException("·« Ì„ﬂ‰ﬂ ≈·€«¡ ÿ·» ·« ÌŒ’ﬂ.");
            }

            if (!order.CanBeCancelled())
            {
                throw new BusinessException(
                    $"·« Ì„ﬂ‰ ≈·€«¡ «·ÿ·» ›Ì Õ«· Â «·Õ«·Ì… ({order.Status}).");
            }

            _logger.LogInformation("Cancelling order {OrderNumber}. Reason: {Reason}",
                order.OrderNumber, request.Reason);

            order.Cancel(request.Reason);
            await _context.SaveChangesAsync();

            // Restore stock if order was paid
            if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Processing)
            {
                await RestoreStockAsync(order);
            }

            return (await _orderRepository.GetByIdWithDetailsAsync(orderId))!;
        }

        public async Task<OrderDto> UpdateTrackingNumberAsync(int orderId, UpdateTrackingRequest request)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("«·ÿ·» €Ì— „ÊÃÊœ.", "Order", orderId);
            }

            _logger.LogInformation("Updating tracking number for order {OrderNumber}",
                order.OrderNumber);

            order.UpdateTrackingNumber(request.TrackingNumber);
            
            // Auto-update status to Shipped if not already
            if (order.Status == OrderStatus.Processing)
            {
                order.UpdateStatus(OrderStatus.Shipped);
            }

            await _context.SaveChangesAsync();

            return (await _orderRepository.GetByIdWithDetailsAsync(orderId))!;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("«·ÿ·» €Ì— „ÊÃÊœ.", "Order", orderId);
            }

            // Only allow deletion of cancelled orders
            if (order.Status != OrderStatus.Cancelled)
            {
                throw new BusinessException("Ì„ﬂ‰ Õ–› «·ÿ·»«  «·„·€«… ›ﬁÿ.");
            }

            order.SoftDelete();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderNumber} soft deleted", order.OrderNumber);

            return true;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _orderRepository.GetTotalRevenueAsync(fromDate, toDate);
        }

        public async Task<int> GetTotalOrdersCountAsync(int? status = null)
        {
            OrderStatus? orderStatus = status.HasValue ? (OrderStatus)status.Value : null;
            return await _orderRepository.GetTotalOrdersCountAsync(orderStatus);
        }

        // Private helper methods
        private async Task ValidateStockAvailabilityAsync(List<CreateOrderItemRequest> items)
        {
            var partIds = items.Select(i => i.PartId).ToList();
            var parts = await _context.CarParts
                .Where(p => partIds.Contains(p.Id))
                .ToListAsync();

            var unavailableItems = new List<string>();

            foreach (var item in items)
            {
                var part = parts.FirstOrDefault(p => p.Id == item.PartId);
                if (part == null)
                {
                    unavailableItems.Add($"«·„‰ Ã (ID: {item.PartId}) €Ì— „ÊÃÊœ");
                    continue;
                }

                if (!part.IsActive || part.IsDeleted)
                {
                    unavailableItems.Add($"{part.PartName} €Ì— „ «Õ");
                    continue;
                }

                if (part.StockQuantity < item.Quantity)
                {
                    unavailableItems.Add(
                        $"{part.PartName} - «·ﬂ„Ì… «·„ «Õ…: {part.StockQuantity}, «·„ÿ·Ê»…: {item.Quantity}");
                }
            }

            if (unavailableItems.Any())
            {
                throw new BusinessException(
                    "»⁄÷ «·„‰ Ã«  €Ì— „ Ê›—… »«·ﬂ„Ì… «·„ÿ·Ê»….",
                    "STOCK_UNAVAILABLE",
                    new Dictionary<string, object> { ["unavailableItems"] = unavailableItems });
            }
        }

        public async Task ReduceStockAsync(Order order)
        {
            foreach (var item in order.OrderItems)
            {
                var part = await _context.CarParts.FindAsync(item.PartId);
                if (part != null)
                {
                    part.ReduceStock(item.Quantity);
                    _logger.LogInformation("Reduced stock for {PartName} by {Quantity}",
                        part.PartName, item.Quantity);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task RestoreStockAsync(Order order)
        {
            foreach (var item in order.OrderItems)
            {
                var part = await _context.CarParts.FindAsync(item.PartId);
                if (part != null)
                {
                    part.AddStock(item.Quantity);
                    _logger.LogInformation("Restored stock for {PartName} by {Quantity}",
                        part.PartName, item.Quantity);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task ClearUserCartAsync(int userId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cart cleared for user {UserId}", userId);
        }
    }
}

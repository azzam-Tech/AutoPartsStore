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
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(int userId, CreateOrderFromCartRequest request)
        {
            _logger.LogInformation("Creating order from cart for user {UserId}", userId);

            // Get user's cart with calculated prices
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

            // ? CALCULATE ORDER TOTALS DIRECTLY FROM CART
            // Cart has already calculated prices following the PRIORITY RULE (Product Discount > Promotion)
            decimal orderSubTotal = cart.TotalPrice;      // Sum of all UnitPrice ◊ Quantity (before discounts)
            decimal orderDiscount = cart.TotalDiscount;   // Sum of all discounts applied
            decimal orderTotal = cart.FinalTotal;         // Final total after all discounts

            _logger.LogInformation(
                "Calculated order totals from cart - SubTotal: {SubTotal} SAR, Discount: {Discount} SAR, Total: {Total} SAR",
                orderSubTotal, orderDiscount, orderTotal);

            // Create order with ACTUAL calculated values from cart (not placeholders!)
            var order = new Order(
                userId,
                request.ShippingAddressId,
                orderSubTotal,
                orderDiscount,
                request.CustomerNotes
            );

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Create order items from cart
            // OrderItem constructor will independently calculate prices to ensure data integrity
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

            // ? VERIFICATION: Ensure OrderItems totals match cart totals
            // Load OrderItems and recalculate to verify consistency
            await _context.Entry(order)
                .Collection(o => o.OrderItems)
                .LoadAsync();
            
            // Store original totals from cart
            var originalSubTotal = order.SubTotal;
            var originalDiscount = order.DiscountAmount;
            var originalTotal = order.TotalAmount;
            
            // Recalculate from OrderItems
            order.RecalculateTotalsFromItems();

            // Check for discrepancies (for debugging/logging)
            if (Math.Abs(order.SubTotal - originalSubTotal) > 0.01m || 
                Math.Abs(order.DiscountAmount - originalDiscount) > 0.01m || 
                Math.Abs(order.TotalAmount - originalTotal) > 0.01m)
            {
                _logger.LogWarning(
                    "Order totals recalculated from items differ from cart! " +
                    "Cart: SubTotal={CartSubTotal}, Discount={CartDiscount}, Total={CartTotal} | " +
                    "Items: SubTotal={ItemsSubTotal}, Discount={ItemsDiscount}, Total={ItemsTotal}",
                    originalSubTotal, originalDiscount, originalTotal,
                    order.SubTotal, order.DiscountAmount, order.TotalAmount);
            }
            else
            {
                _logger.LogInformation("? Order totals verified - Cart and OrderItems match perfectly!");
            }

            await _context.SaveChangesAsync();

            // Clear the cart
            await ClearUserCartAsync(userId);

            _logger.LogInformation(
                "Order {OrderNumber} created successfully for user {UserId}. " +
                "SubTotal: {SubTotal} SAR, Discount: {Discount} SAR, Total: {Total} SAR",
                order.OrderNumber, userId, order.SubTotal, order.DiscountAmount, order.TotalAmount);

            // Return complete order details
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

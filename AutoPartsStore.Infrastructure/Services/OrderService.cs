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
        private const decimal VAT_RATE = 0.15m; // 15% VAT
        private const decimal DEFAULT_SHIPPING_COST = 25.00m; // Default shipping in SAR

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

            // Calculate order totals
            var (subTotal, discountAmount, taxAmount, shippingCost) = CalculateOrderTotals(cart.Items);

            // Create order
            var order = new Order(
                userId,
                request.ShippingAddressId,
                subTotal,
                discountAmount,
                taxAmount,
                shippingCost,
                request.CustomerNotes
            );

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Create order items from cart
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
            // This ensures order header matches the sum of order items
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            decimal actualSubTotal = orderItems.Sum(oi => oi.SubTotal);
            decimal actualDiscount = orderItems.Sum(oi => oi.DiscountAmount);
            decimal actualTax = (actualSubTotal - actualDiscount) * VAT_RATE;

            order.UpdateTotals(actualSubTotal, actualDiscount, actualTax);

            await _context.SaveChangesAsync();

            // Clear the cart
            await ClearUserCartAsync(userId);

            _logger.LogInformation("Order {OrderNumber} created successfully for user {UserId}",
                order.OrderNumber, userId);

            // Return order details
            return (await _orderRepository.GetByIdWithDetailsAsync(order.Id))!;
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                throw new ValidationException("ÌÃ»  ÕœÌœ ⁄‰«’— «·ÿ·».");
            }

            _logger.LogInformation("Creating direct order for user {UserId}", userId);

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

            // Validate stock
            await ValidateStockAvailabilityAsync(request.Items);

            // Get parts and calculate totals
            var parts = await _context.CarParts
                .Include(p => p.Promotion)
                .Where(p => request.Items.Select(i => i.PartId).Contains(p.Id))
                .ToListAsync();

            decimal subTotal = 0;
            decimal totalDiscount = 0;

            var orderItemsData = new List<(CarPart part, int quantity)>();

            foreach (var item in request.Items)
            {
                var part = parts.FirstOrDefault(p => p.Id == item.PartId);
                if (part == null)
                {
                    throw new NotFoundException($"«·„‰ Ã €Ì— „ÊÃÊœ.", "CarPart", item.PartId);
                }

                if (!part.IsActive || part.IsDeleted)
                {
                    throw new BusinessException($"«·„‰ Ã '{part.PartName}' €Ì— „ «Õ.");
                }

                orderItemsData.Add((part, item.Quantity));

                var itemSubTotal = part.UnitPrice * item.Quantity;
                var itemFinalPrice = part.GetFinalPrice() * item.Quantity;
                
                subTotal += itemSubTotal;
                totalDiscount += (itemSubTotal - itemFinalPrice);
            }

            var taxAmount = (subTotal - totalDiscount) * VAT_RATE;
            var shippingCost = DEFAULT_SHIPPING_COST;

            // Create order
            var order = new Order(
                userId,
                request.ShippingAddressId,
                subTotal,
                totalDiscount,
                taxAmount,
                shippingCost,
                request.CustomerNotes
            );

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Create order items
            foreach (var (part, quantity) in orderItemsData)
            {
                var orderItem = new OrderItem(
                    order.Id,
                    part.Id,
                    part.PartNumber,
                    part.PartName,
                    part.UnitPrice,
                    part.DiscountPercent,
                    quantity,
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
            // This ensures order header matches the sum of order items
            var orderItemsList = await _context.OrderItems
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            decimal actualSubTotal = orderItemsList.Sum(oi => oi.SubTotal);
            decimal actualDiscount = orderItemsList.Sum(oi => oi.DiscountAmount);
            decimal actualTax = (actualSubTotal - actualDiscount) * VAT_RATE;

            order.UpdateTotals(actualSubTotal, actualDiscount, actualTax);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderNumber} created successfully", order.OrderNumber);

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

        public async Task<decimal> CalculateOrderTotalAsync(List<CreateOrderItemRequest> items)
        {
            if (!items.Any())
                return 0;

            var partIds = items.Select(i => i.PartId).ToList();
            var parts = await _context.CarParts
                .Where(p => partIds.Contains(p.Id))
                .ToListAsync();

            decimal subTotal = 0;
            decimal totalDiscount = 0;

            foreach (var item in items)
            {
                var part = parts.FirstOrDefault(p => p.Id == item.PartId);
                if (part != null)
                {
                    var itemSubTotal = part.UnitPrice * item.Quantity;
                    var itemFinalPrice = part.GetFinalPrice() * item.Quantity;
                    
                    subTotal += itemSubTotal;
                    totalDiscount += (itemSubTotal - itemFinalPrice);
                }
            }

            var taxAmount = (subTotal - totalDiscount) * VAT_RATE;
            var totalAmount = (subTotal - totalDiscount) + taxAmount + DEFAULT_SHIPPING_COST;

            return totalAmount;
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
        private (decimal subTotal, decimal discount, decimal tax, decimal shipping) CalculateOrderTotals(
            List<Core.Models.Cart.CartItemDto> items)
        {
            decimal subTotal = items.Sum(i => i.TotalPrice);
            decimal totalDiscount = items.Sum(i => i.TotalDiscount);
            decimal taxAmount = (subTotal - totalDiscount) * VAT_RATE;
            decimal shippingCost = DEFAULT_SHIPPING_COST;

            return (subTotal, totalDiscount, taxAmount, shippingCost);
        }

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

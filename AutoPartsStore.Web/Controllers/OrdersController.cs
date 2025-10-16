using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Create order from shopping cart
        /// </summary>
        [HttpPost("from-cart")]
        [Authorize]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CreateOrderRequest request)
        {
            var userId = GetAuthenticatedUserId();
            var order = await _orderService.CreateOrderFromCartAsync(userId, request);
            return Success(order, " „ ≈‰‘«¡ «·ÿ·» »‰Ã«Õ „‰ ”·… «· ”Êﬁ.");
        }

        /// <summary>
        /// Create order directly (without cart)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = GetAuthenticatedUserId();
            var order = await _orderService.CreateOrderAsync(userId, request);
            return Success(order, " „ ≈‰‘«¡ «·ÿ·» »‰Ã«Õ.");
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound("«·ÿ·» €Ì— „ÊÃÊœ.");

            var userId = GetAuthenticatedUserId();
            var isAdmin = User.IsInRole("Admin");

            // Users can only see their own orders, admins can see all
            if (order.UserId != userId && !isAdmin)
                return Forbid();

            return Success(order);
        }

        /// <summary>
        /// Get order by order number
        /// </summary>
        [HttpGet("number/{orderNumber}")]
        [Authorize]
        public async Task<IActionResult> GetOrderByNumber(string orderNumber)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null)
                return NotFound("«·ÿ·» €Ì— „ÊÃÊœ.");

            var userId = GetAuthenticatedUserId();
            var isAdmin = User.IsInRole("Admin");

            if (order.UserId != userId && !isAdmin)
                return Forbid();

            return Success(order);
        }

        /// <summary>
        /// Get all orders (Admin only) with filtering
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrders([FromQuery] OrderFilterRequest filter)
        {
            var orders = await _orderService.GetOrdersAsync(filter);
            return Success(orders);
        }

        /// <summary>
        /// Get user's orders
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            var isAdmin = User.IsInRole("Admin");

            // Users can only see their own orders
            if (userId != authenticatedUserId && !isAdmin)
                return Forbid();

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Success(orders);
        }

        /// <summary>
        /// Get authenticated user's orders
        /// </summary>
        [HttpGet("my-orders")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetAuthenticatedUserId();
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Success(orders);
        }

        /// <summary>
        /// Get recent orders (Admin only)
        /// </summary>
        [HttpGet("recent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int count = 10)
        {
            var orders = await _orderService.GetRecentOrdersAsync(count);
            return Success(orders);
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, request);
            return Success(order, " „  ÕœÌÀ Õ«·… «·ÿ·» »‰Ã«Õ.");
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
        {
            var userId = GetAuthenticatedUserId();
            var order = await _orderService.CancelOrderAsync(id, userId, request);
            return Success(order, " „ ≈·€«¡ «·ÿ·» »‰Ã«Õ.");
        }

        /// <summary>
        /// Update tracking number (Admin only)
        /// </summary>
        [HttpPatch("{id}/tracking")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTracking(int id, [FromBody] UpdateTrackingRequest request)
        {
            var order = await _orderService.UpdateTrackingNumberAsync(id, request);
            return Success(order, " „  ÕœÌÀ —ﬁ„ «·  »⁄ »‰Ã«Õ.");
        }

        /// <summary>
        /// Delete order (Admin only, soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            await _orderService.DeleteOrderAsync(id);
            return Success(" „ Õ–› «·ÿ·» »‰Ã«Õ.");
        }

        /// <summary>
        /// Calculate order total
        /// </summary>
        [HttpPost("calculate-total")]
        [Authorize]
        public async Task<IActionResult> CalculateTotal([FromBody] List<CreateOrderItemRequest> items)
        {
            var total = await _orderService.CalculateOrderTotalAsync(items);
            return Success(new { total }, " „ Õ”«» ≈Ã„«·Ì «·ÿ·».");
        }

        /// <summary>
        /// Get order statistics (Admin only)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var totalRevenue = await _orderService.GetTotalRevenueAsync(fromDate, toDate);
            var totalOrders = await _orderService.GetTotalOrdersCountAsync(null);
            var pendingOrders = await _orderService.GetTotalOrdersCountAsync((int)OrderStatus.Pending);
            var paidOrders = await _orderService.GetTotalOrdersCountAsync((int)OrderStatus.Paid);
            var processingOrders = await _orderService.GetTotalOrdersCountAsync((int)OrderStatus.Processing);
            var shippedOrders = await _orderService.GetTotalOrdersCountAsync((int)OrderStatus.Shipped);
            var deliveredOrders = await _orderService.GetTotalOrdersCountAsync((int)OrderStatus.Delivered);
            var cancelledOrders = await _orderService.GetTotalOrdersCountAsync((int)OrderStatus.Cancelled);

            var statistics = new
            {
                totalRevenue,
                totalOrders,
                ordersByStatus = new
                {
                    pending = pendingOrders,
                    paid = paidOrders,
                    processing = processingOrders,
                    shipped = shippedOrders,
                    delivered = deliveredOrders,
                    cancelled = cancelledOrders
                }
            };

            return Success(statistics);
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("„⁄—¯› «·„” Œœ„ €Ì— „ÊÃÊœ.");
            return int.Parse(userIdClaim);
        }
    }
}

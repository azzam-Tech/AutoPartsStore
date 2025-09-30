using AutoPartsStore.Core.Models.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class ShoppingCartController : BaseController
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(
            IShoppingCartService shoppingCartService,
            ILogger<ShoppingCartController> logger)
        {
            _shoppingCartService = shoppingCartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetAuthenticatedUserId();
            var cart = await _shoppingCartService.GetUserCartAsync(userId);
            if (cart == null)
            {
                return NotFound("Shopping cart not found.");
            }
            return Success(cart);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetCartSummary()
        {
            var userId = GetAuthenticatedUserId();
            var summary = await _shoppingCartService.GetCartSummaryAsync(userId);
            return Success(summary);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                var result = await _shoppingCartService.AddItemToCartAsync(userId, request);
                return Success(result, "Item added to cart successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateCartItem(int itemId, [FromBody] UpdateCartItemRequest request)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                var result = await _shoppingCartService.UpdateCartItemAsync(userId, itemId, request);
                return Success(result, "Cart item updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                await _shoppingCartService.RemoveItemFromCartAsync(userId, itemId);
                return Success("Item removed from cart successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                await _shoppingCartService.ClearCartAsync(userId);
                return Success("Cart cleared successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("transfer/{toUserId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TransferCart(int toUserId)
        {
            var fromUserId = GetAuthenticatedUserId();

            try
            {
                await _shoppingCartService.TransferCartAsync(fromUserId, toUserId);
                return Success("Cart transferred successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim);
        }
    }
}
using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Cart;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<ShoppingCartService> _logger;

        public ShoppingCartService(
            IShoppingCartRepository shoppingCartRepository,
            ICartItemRepository cartItemRepository,
            AppDbContext context,
            ILogger<ShoppingCartService> logger)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _cartItemRepository = cartItemRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<ShoppingCartDto> GetUserCartAsync(int userId)
        {
            return await _shoppingCartRepository.GetCartByUserIdAsync(userId);
        }

        public async Task<CartItemDto> AddItemToCartAsync(int userId, AddToCartRequest request)
        {
            // الحصول على عربة التسوق أو إنشاؤها إذا لم تكن موجودة
            var cart = await GetOrCreateCartAsync(userId);

            // التحقق من وجود المنتج
            var carPart = await _context.CarParts
                .FirstOrDefaultAsync(p => p.Id == request.PartId && p.IsActive && !p.IsDeleted);

            if (carPart == null)
                throw new KeyNotFoundException("Product not found or unavailable");

            // التحقق من المخزون
            if (carPart.StockQuantity < request.Quantity)
                throw new InvalidOperationException($"Not enough stock. Available: {carPart.StockQuantity}");

            // التحقق إذا كان المنتج موجوداً بالفعل في العربة
            var existingItem = await _shoppingCartRepository.GetCartItemAsync(cart.Id, request.PartId);

            if (existingItem != null)
            {
                // تحديث الكمية إذا كان المنتج موجوداً
                existingItem.UpdateQuantity(existingItem.Quantity + request.Quantity);
                _context.CartItems.Update(existingItem);
            }
            else
            {
                // إضافة منتج جديد
                var cartItem = new CartItem(cart.Id, request.PartId, request.Quantity);
                await _context.CartItems.AddAsync(cartItem);
            }

            // تحديث وقت التعديل
            cart.UpdateLastUpdated();
            _context.ShoppingCarts.Update(cart);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Item {PartId} added to cart for user {UserId}", request.PartId, userId);
            return await _cartItemRepository.GetCartItemDetailsAsync(
                (await _shoppingCartRepository.GetCartItemAsync(cart.Id, request.PartId)).Id);
        }

        public async Task<CartItemDto> UpdateCartItemAsync(int userId, int itemId, UpdateCartItemRequest request)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.CarPart)
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.Cart.UserId == userId);

            if (cartItem == null)
                throw new KeyNotFoundException("Cart item not found");

            if (cartItem.CarPart.StockQuantity < request.Quantity)
                throw new InvalidOperationException($"Not enough stock. Available: {cartItem.CarPart.StockQuantity}");

            cartItem.UpdateQuantity(request.Quantity);
            cartItem.Cart.UpdateLastUpdated();

            _context.CartItems.Update(cartItem);
            _context.ShoppingCarts.Update(cartItem.Cart);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cart item {ItemId} updated for user {UserId}", itemId, userId);
            return await _cartItemRepository.GetCartItemDetailsAsync(itemId);
        }

        public async Task<bool> RemoveItemFromCartAsync(int userId, int itemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.Cart.UserId == userId);

            if (cartItem == null)
                throw new KeyNotFoundException("Cart item not found");

            _context.CartItems.Remove(cartItem);
            cartItem.Cart.UpdateLastUpdated();
            _context.ShoppingCarts.Update(cartItem.Cart);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cart item {ItemId} removed for user {UserId}", itemId, userId);
            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(sc => sc.Items)
                .FirstOrDefaultAsync(sc => sc.UserId == userId);

            if (cart == null) return false;

            _context.CartItems.RemoveRange(cart.Items);
            cart.UpdateLastUpdated();
            _context.ShoppingCarts.Update(cart);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cart cleared for user {UserId}", userId);
            return true;
        }

        public async Task<CartSummaryDto> GetCartSummaryAsync(int userId)
        {
            var cart = await _shoppingCartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return new CartSummaryDto();

            return new CartSummaryDto
            {
                TotalItems = cart.TotalItems,
                TotalPrice = cart.Items.Sum(i => i.TotalPrice),
                TotalDiscount = cart.Items.Sum(i => i.TotalDiscount),
                FinalTotal = cart.Items.Sum(i => i.FinalTotal)
            };
        }

        public async Task<bool> TransferCartAsync(int fromUserId, int toUserId)
        {
            var fromCart = await _context.ShoppingCarts
                .Include(sc => sc.Items)
                .FirstOrDefaultAsync(sc => sc.UserId == fromUserId);

            var toCart = await GetOrCreateCartAsync(toUserId);

            if (fromCart?.Items.Any() != true) return false;

            foreach (var item in fromCart.Items)
            {
                var existingItem = await _shoppingCartRepository.GetCartItemAsync(toCart.Id, item.PartId);

                if (existingItem != null)
                {
                    existingItem.UpdateQuantity(existingItem.Quantity + item.Quantity);
                    _context.CartItems.Update(existingItem);
                }
                else
                {
                    var newItem = new CartItem(toCart.Id, item.PartId, item.Quantity);
                    await _context.CartItems.AddAsync(newItem);
                }
            }

            // حذف العربة القديمة
            _context.ShoppingCarts.Remove(fromCart);
            toCart.UpdateLastUpdated();
            _context.ShoppingCarts.Update(toCart);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cart transferred from user {FromUserId} to user {ToUserId}", fromUserId, toUserId);
            return true;
        }

        private async Task<ShoppingCart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(sc => sc.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart(userId);
                await _context.ShoppingCarts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }
    }
}
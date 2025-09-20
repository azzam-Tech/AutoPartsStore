using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Cart;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class ShoppingCartRepository : BaseRepository<ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(AppDbContext context) : base(context) { }

        public async Task<ShoppingCartDto> GetCartByUserIdAsync(int userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(sc => sc.User)
                .Include(sc => sc.Items)
                    .ThenInclude(ci => ci.CarPart)
                .FirstOrDefaultAsync(sc => sc.UserId == userId);

            if (cart == null) return null;

            return new ShoppingCartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                UserName = cart.User.FullName,
                CreatedDate = cart.CreatedDate,
                LastUpdated = cart.LastUpdated,
                TotalItems = cart.Items.Sum(ci => ci.Quantity),
                Items = cart.Items.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    CartId = ci.CartId,
                    PartId = ci.PartId,
                    PartNumber = ci.CarPart.PartNumber,
                    PartName = ci.CarPart.PartName,
                    ImageUrl = ci.CarPart.ImageUrl,
                    UnitPrice = ci.CarPart.UnitPrice,
                    DiscountPercent = ci.CarPart.DiscountPercent,
                    FinalPrice = ci.CarPart.GetFinalPrice(),
                    Quantity = ci.Quantity,
                    TotalPrice = ci.CarPart.UnitPrice * ci.Quantity,
                    TotalDiscount = (ci.CarPart.UnitPrice * ci.CarPart.DiscountPercent / 100) * ci.Quantity,
                    FinalTotal = ci.CarPart.GetFinalPrice() * ci.Quantity,
                    CreatedAt = ci.CreatedAt,
                    IsAvailable = ci.CarPart.IsInStock(),
                    AvailableStock = ci.CarPart.StockQuantity
                }).ToList()
            };
        }

        public async Task<CartItem> GetCartItemAsync(int cartId, int partId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.PartId == partId);
        }

        public async Task<bool> CartContainsItemAsync(int cartId, int partId)
        {
            return await _context.CartItems
                .AnyAsync(ci => ci.CartId == cartId && ci.PartId == partId);
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            return await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }
    }
}
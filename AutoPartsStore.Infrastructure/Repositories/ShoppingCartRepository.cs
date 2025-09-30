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
            return await _context.ShoppingCarts
                .Where(sc => sc.UserId == userId)
                .Select(cart => new ShoppingCartDto
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    UserName = cart.User.FullName,
                    CreatedDate = cart.CreatedDate,
                    LastUpdated = cart.LastUpdated,
                    TotalItems = cart.Items.Sum(ci => ci.Quantity),
                    TotalPrice = cart.Items.Sum(ci => ci.CarPart.UnitPrice * ci.Quantity),
                    TotalDiscount = cart.Items.Sum(ci => (ci.CarPart.UnitPrice * ci.CarPart.DiscountPercent / 100) * ci.Quantity),
                    FinalTotal = cart.Items.Sum(ci => ci.CarPart.UnitPrice * ci.Quantity) - cart.Items.Sum(ci => (ci.CarPart.UnitPrice * ci.CarPart.DiscountPercent / 100) * ci.Quantity),
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
                        HasPromotion = ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow(),
                        PromotionName = ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow() ? ci.CarPart.Promotion.PromotionName : null,
                        DiscountType = ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow() ? ci.CarPart.Promotion.DiscountType : null,
                        DiscountValue = ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow() ? ci.CarPart.Promotion.DiscountValue : 0,
                        FinalPrice = ci.CarPart.GetFinalPrice(),
                        Quantity = ci.Quantity,
                        TotalPrice = ci.CarPart.UnitPrice * ci.Quantity,
                        TotalDiscount = (ci.CarPart.UnitPrice * ci.CarPart.DiscountPercent / 100) * ci.Quantity,
                        FinalTotal = ci.CarPart.GetFinalPrice() * ci.Quantity,
                        CreatedAt = ci.CreatedAt,
                        IsAvailable = ci.CarPart.IsInStock(),
                        AvailableStock = ci.CarPart.StockQuantity
                    }).ToList()
            }).FirstOrDefaultAsync() ?? throw new DirectoryNotFoundException("Car part not found.");
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int partId)
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
using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Cart;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class CartItemRepository : BaseRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(AppDbContext context) : base(context) { }

        public async Task<List<CartItemDto>> GetCartItemsAsync(int cartId)
        {
            return await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    CartId = ci.CartId,
                    PartId = ci.PartId,
                    PartNumber = ci.CarPart.PartNumber,
                    PartName = ci.CarPart.PartName,
                    ImageUrl = ci.CarPart.ImageUrl,
                    UnitPrice = ci.CarPart.UnitPrice,
                    DiscountPercent = ci.CarPart.DiscountPercent,
                    // PRIORITY RULE: If product has discount, use it. Otherwise use promotion.
                    FinalPrice = ci.CarPart.DiscountPercent > 0
                        ? ci.CarPart.UnitPrice * (1 - ci.CarPart.DiscountPercent / 100)
                        : (ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow()
                            ? (ci.CarPart.Promotion.DiscountType == DiscountType.Percent
                                ? ci.CarPart.UnitPrice * (1 - ci.CarPart.Promotion.DiscountValue / 100)
                                : Math.Max(0, ci.CarPart.UnitPrice - ci.CarPart.Promotion.DiscountValue))
                            : ci.CarPart.UnitPrice),
                    Quantity = ci.Quantity,
                    TotalPrice = ci.CarPart.UnitPrice * ci.Quantity,
                    TotalDiscount = ci.CarPart.DiscountPercent > 0
                        ? (ci.CarPart.UnitPrice * ci.CarPart.DiscountPercent / 100) * ci.Quantity
                        : (ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow()
                            ? (ci.CarPart.Promotion.DiscountType == DiscountType.Percent
                                ? (ci.CarPart.UnitPrice * ci.CarPart.Promotion.DiscountValue / 100) * ci.Quantity
                                : ci.CarPart.Promotion.DiscountValue * ci.Quantity)
                            : 0),
                    FinalTotal = ci.CarPart.DiscountPercent > 0
                        ? ci.CarPart.UnitPrice * (1 - ci.CarPart.DiscountPercent / 100) * ci.Quantity
                        : (ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow()
                            ? (ci.CarPart.Promotion.DiscountType == DiscountType.Percent
                                ? ci.CarPart.UnitPrice * (1 - ci.CarPart.Promotion.DiscountValue / 100) * ci.Quantity
                                : Math.Max(0, ci.CarPart.UnitPrice - ci.CarPart.Promotion.DiscountValue) * ci.Quantity)
                            : ci.CarPart.UnitPrice * ci.Quantity),
                    CreatedAt = ci.CreatedAt,
                    IsAvailable = ci.CarPart.IsInStock(),
                    AvailableStock = ci.CarPart.StockQuantity
                })
                .ToListAsync();
        }

        public async Task<CartItemDto> GetCartItemDetailsAsync(int cartItemId)
        {
            return await _context.CartItems
                .Where(ci => ci.Id == cartItemId)
                .Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    CartId = ci.CartId,
                    PartId = ci.PartId,
                    PartNumber = ci.CarPart.PartNumber,
                    PartName = ci.CarPart.PartName,
                    ImageUrl = ci.CarPart.ImageUrl,
                    UnitPrice = ci.CarPart.UnitPrice,
                    DiscountPercent = ci.CarPart.DiscountPercent,
                    // PRIORITY RULE: If product has discount, use it. Otherwise use promotion.
                    FinalPrice = ci.CarPart.DiscountPercent > 0
                        ? ci.CarPart.UnitPrice * (1 - ci.CarPart.DiscountPercent / 100)
                        : (ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow()
                            ? (ci.CarPart.Promotion.DiscountType == DiscountType.Percent
                                ? ci.CarPart.UnitPrice * (1 - ci.CarPart.Promotion.DiscountValue / 100)
                                : Math.Max(0, ci.CarPart.UnitPrice - ci.CarPart.Promotion.DiscountValue))
                            : ci.CarPart.UnitPrice),
                    Quantity = ci.Quantity,
                    TotalPrice = ci.CarPart.UnitPrice * ci.Quantity,
                    TotalDiscount = ci.CarPart.DiscountPercent > 0
                        ? (ci.CarPart.UnitPrice * ci.CarPart.DiscountPercent / 100) * ci.Quantity
                        : (ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow()
                            ? (ci.CarPart.Promotion.DiscountType == DiscountType.Percent
                                ? (ci.CarPart.UnitPrice * ci.CarPart.Promotion.DiscountValue / 100) * ci.Quantity
                                : ci.CarPart.Promotion.DiscountValue * ci.Quantity)
                            : 0),
                    FinalTotal = ci.CarPart.DiscountPercent > 0
                        ? ci.CarPart.UnitPrice * (1 - ci.CarPart.DiscountPercent / 100) * ci.Quantity
                        : (ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow()
                            ? (ci.CarPart.Promotion.DiscountType == DiscountType.Percent
                                ? ci.CarPart.UnitPrice * (1 - ci.CarPart.Promotion.DiscountValue / 100) * ci.Quantity
                                : Math.Max(0, ci.CarPart.UnitPrice - ci.CarPart.Promotion.DiscountValue) * ci.Quantity)
                            : ci.CarPart.UnitPrice * ci.Quantity),
                    CreatedAt = ci.CreatedAt,
                    IsAvailable = ci.CarPart.IsInStock(),
                    AvailableStock = ci.CarPart.StockQuantity
                })
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> CalculateCartTotalAsync(int cartId)
        {
            return await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .Include(ci => ci.CarPart)
                .ThenInclude(cp => cp.Promotion)
                .SumAsync(ci => 
                    // PRIORITY RULE: If product has discount, use it. Otherwise use promotion.
                    ci.CarPart.DiscountPercent > 0
                        ? ci.CarPart.UnitPrice * (1 - ci.CarPart.DiscountPercent / 100) * ci.Quantity
                        : (ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow()
                            ? (ci.CarPart.Promotion.DiscountType == DiscountType.Percent
                                ? ci.CarPart.UnitPrice * (1 - ci.CarPart.Promotion.DiscountValue / 100) * ci.Quantity
                                : Math.Max(0, ci.CarPart.UnitPrice - ci.CarPart.Promotion.DiscountValue) * ci.Quantity)
                            : ci.CarPart.UnitPrice * ci.Quantity));
        }
    }
}
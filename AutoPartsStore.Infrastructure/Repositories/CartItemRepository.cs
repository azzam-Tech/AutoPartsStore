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
                .Include(ci => ci.CarPart)
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
                    FinalPrice = ci.CarPart.GetFinalPrice(),
                    Quantity = ci.Quantity,
                    TotalPrice = ci.CarPart.UnitPrice * ci.Quantity,
                    TotalDiscount = (ci.CarPart.UnitPrice * ci.CarPart.DiscountPercent / 100) * ci.Quantity,
                    FinalTotal = ci.CarPart.GetFinalPrice() * ci.Quantity,
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
                .Include(ci => ci.CarPart)
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
                    FinalPrice = ci.CarPart.GetFinalPrice(),
                    Quantity = ci.Quantity,
                    TotalPrice = ci.CarPart.UnitPrice * ci.Quantity,
                    TotalDiscount = (ci.CarPart.UnitPrice * ci.CarPart.DiscountPercent / 100) * ci.Quantity,
                    FinalTotal = ci.CarPart.GetFinalPrice() * ci.Quantity,
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
                .SumAsync(ci => ci.CarPart.GetFinalPrice() * ci.Quantity);
        }
    }
}
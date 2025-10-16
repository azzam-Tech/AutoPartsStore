using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Models.Favorites;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class FavoriteRepository : BaseRepository<Favorite>, IFavoriteRepository
    {
        public FavoriteRepository(AppDbContext context) : base(context) { }

        public async Task<List<FavoriteDto>> GetUserFavoritesAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.AddedDate)
                .Select(f => new FavoriteDto
                {
                    Id = f.Id,
                    PartId = f.PartId,
                    PartNumber = f.CarPart.PartNumber,
                    PartName = f.CarPart.PartName,
                    ImageUrl = f.CarPart.ImageUrl,
                    UnitPrice = f.CarPart.UnitPrice,
                    DiscountPercent = f.CarPart.DiscountPercent,
                    FinalPrice = f.CarPart.GetFinalPrice(),
                    IsInStock = f.CarPart.IsInStock(),
                    AddedDate = f.AddedDate
                })
                .ToListAsync();
        }

        public async Task<bool> IsProductInFavoritesAsync(int userId, int partId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.PartId == partId);
        }

        public async Task<int> GetFavoriteCountAsync(int userId)
        {
            return await _context.Favorites
                .CountAsync(f => f.UserId == userId);
        }
    }
}

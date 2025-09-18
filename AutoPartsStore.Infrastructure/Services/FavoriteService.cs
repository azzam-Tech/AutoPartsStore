using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Favorites;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(
            IFavoriteRepository favoriteRepository,
            AppDbContext context,
            ILogger<FavoriteService> logger)
        {
            _favoriteRepository = favoriteRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<List<FavoriteDto>> GetUserFavoritesAsync(int userId)
        {
            return await _favoriteRepository.GetUserFavoritesAsync(userId);
        }

        public async Task<bool> AddToFavoritesAsync(int userId, AddToFavoriteRequest request)
        {
            // التحقق من وجود المنتج
            var part = await _context.CarParts
                .FirstOrDefaultAsync(p => p.Id == request.PartId && p.IsActive && !p.IsDeleted);

            if (part == null)
                throw new KeyNotFoundException("Product not found or unavailable");

            // التحقق إذا كان المنتج موجوداً بالفعل في المفضلة
            if (await _favoriteRepository.IsProductInFavoritesAsync(userId, request.PartId))
                throw new InvalidOperationException("Product is already in favorites");

            var favorite = new Favorite(userId, request.PartId);
            await _context.Favorites.AddAsync(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {PartId} added to favorites by user {UserId}", request.PartId, userId);
            return true;
        }

        public async Task<bool> RemoveFromFavoritesAsync(int userId, int partId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PartId == partId);

            if (favorite == null)
                throw new KeyNotFoundException("Product not found in favorites");

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {PartId} removed from favorites by user {UserId}", partId, userId);
            return true;
        }

        public async Task<bool> IsProductInFavoritesAsync(int userId, int partId)
        {
            return await _favoriteRepository.IsProductInFavoritesAsync(userId, partId);
        }

        public async Task<int> GetFavoriteCountAsync(int userId)
        {
            return await _favoriteRepository.GetFavoriteCountAsync(userId);
        }
    }
}

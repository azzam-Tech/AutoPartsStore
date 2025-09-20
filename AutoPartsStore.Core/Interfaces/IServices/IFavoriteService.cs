using AutoPartsStore.Core.Models.Favorites;

namespace AutoPartsStore.Core.Interfaces.IServices
{
    public interface IFavoriteService
    {
        Task<List<FavoriteDto>> GetUserFavoritesAsync(int userId);
        Task<bool> AddToFavoritesAsync(int userId, AddToFavoriteRequest request);
        Task<bool> RemoveFromFavoritesAsync(int userId, int partId);
        Task<bool> IsProductInFavoritesAsync(int userId, int partId);
        Task<int> GetFavoriteCountAsync(int userId);
    }
}

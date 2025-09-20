using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Favorites;

namespace AutoPartsStore.Core.Interfaces.IRepositories
{
    public interface IFavoriteRepository : IBaseRepository<Favorite>
    {
        Task<List<FavoriteDto>> GetUserFavoritesAsync(int userId);
        Task<bool> IsProductInFavoritesAsync(int userId, int partId);
        Task<int> GetFavoriteCountAsync(int userId);
    }
}

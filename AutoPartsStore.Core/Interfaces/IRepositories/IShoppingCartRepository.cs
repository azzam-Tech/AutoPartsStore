using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Cart;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IShoppingCartRepository : IBaseRepository<ShoppingCart>
    {
        Task<ShoppingCartDto> GetCartByUserIdAsync(int userId);
        Task<CartItem> GetCartItemAsync(int cartId, int partId);
        Task<bool> CartContainsItemAsync(int cartId, int partId);
        Task<int> GetCartItemCountAsync(int userId);
    }

 
}
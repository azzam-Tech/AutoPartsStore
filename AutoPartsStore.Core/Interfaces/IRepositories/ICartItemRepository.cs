using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Cart;

namespace AutoPartsStore.Core.Interfaces
{
    public interface ICartItemRepository : IBaseRepository<CartItem>
    {
        Task<List<CartItemDto>> GetCartItemsAsync(int cartId);
        Task<CartItemDto> GetCartItemDetailsAsync(int cartItemId);
        Task<decimal> CalculateCartTotalAsync(int cartId);
    }

 
}
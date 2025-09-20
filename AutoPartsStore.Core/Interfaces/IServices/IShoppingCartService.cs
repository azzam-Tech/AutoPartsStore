using AutoPartsStore.Core.Models.Cart;

public interface IShoppingCartService
{
    Task<ShoppingCartDto> GetUserCartAsync(int userId);
    Task<CartItemDto> AddItemToCartAsync(int userId, AddToCartRequest request);
    Task<CartItemDto> UpdateCartItemAsync(int userId, int itemId, UpdateCartItemRequest request);
    Task<bool> RemoveItemFromCartAsync(int userId, int itemId);
    Task<bool> ClearCartAsync(int userId);
    Task<CartSummaryDto> GetCartSummaryAsync(int userId);
    Task<bool> TransferCartAsync(int fromUserId, int toUserId);
}
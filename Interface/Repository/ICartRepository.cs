using backend.Dto;
using backend.Models;

public interface ICartRepository
{
    Task<Cart> GetCartByUserIdAsync(string userId);
    Task AddOrUpdateItemAsync(string userId, CartItemDto itemDto);
    Task RemoveItemAsync(string userId, int productId);
    Task ClearCartAsync(string userId);
    Task SaveChangesAsync();
}

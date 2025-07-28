using backend.Dto;
namespace backend.Interface.Service;

public interface ICartService
{
    Task<CartReadDto> GetUserCartAsync(string userId);
    Task AddToCartAsync(string userId, CartItemDto itemDto);
    Task RemoveFromCartAsync(string userId, int productId);
    Task ClearCartAsync(string userId);
    Task UpdateItemQuantityAsync(string userId, int productId, int quantity);
}

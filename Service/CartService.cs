using backend.Dto;
using backend.Interface.Service;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Service;

public class CartService : ICartService
{
    private readonly ICartRepository _repo;
    private readonly AppDbContext _context; // inject this for product check

    public CartService(ICartRepository repo, AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<CartReadDto> GetUserCartAsync(string userId)
    {
        var cart = await _repo.GetCartByUserIdAsync(userId);
        return new CartReadDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(i => new CartItemDetailsDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Title,
                Price = i.Product.Price,
                Quantity = i.Quantity
            }).ToList()
        };
    }

    public async Task AddToCartAsync(string userId, CartItemDto itemDto)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == itemDto.ProductId);
        if (!productExists)
        {
            throw new ArgumentException("Product does not exist.");
        }

        await _repo.AddOrUpdateItemAsync(userId, itemDto);
    }

    public async Task RemoveFromCartAsync(string userId, int productId)
    {
        await _repo.RemoveItemAsync(userId, productId);
    }

    public async Task ClearCartAsync(string userId)
    {
        await _repo.ClearCartAsync(userId);
    }
        public async Task UpdateItemQuantityAsync(string userId, int productId, int quantity)
    {
        await _repo.UpdateItemQuantityAsync(userId, productId, quantity);
    }
}

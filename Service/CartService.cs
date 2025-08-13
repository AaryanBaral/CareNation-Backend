using backend.Dto;
using backend.Interface.Service;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Interface.Repository;

namespace backend.Service;

public class CartService : ICartService
{
    private readonly ICartRepository _repo;
    private readonly AppDbContext _context; // inject this for product check,
    private readonly IProductImageRepository _productImageRepository;

    public CartService(ICartRepository repo, AppDbContext context, IProductImageRepository productImageRepository)
    {
        _repo = repo;
        _context = context;
        _productImageRepository = productImageRepository;
    }

    public async Task<CartReadDto> GetUserCartAsync(string userId)
    {
        var cart = await _repo.GetCartByUserIdAsync(userId);

        return new CartReadDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = [.. await Task.WhenAll(cart.Items.Select(async i => {
                var imageUrls = await _productImageRepository.GetProductImagesByProductIdAsync(i.ProductId);
                    return new CartItemDetailsDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Title,
                        Price = i.Product.RetailPrice,
                        Quantity = i.Quantity,
                        ImageUrls = [.. imageUrls.Select(img => img.ImageUrl)]
                    };
                }))]
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

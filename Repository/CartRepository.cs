using backend.Data;
using backend.Dto;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository;
public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cart> GetCartByUserIdAsync(string userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    public async Task AddOrUpdateItemAsync(string userId, CartItemDto itemDto)
    {
        var cart = await GetCartByUserIdAsync(userId);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == itemDto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += itemDto.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(string userId, int productId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item != null)
        {
            cart.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        cart.Items.Clear();
        await _context.SaveChangesAsync();
    }
    public async Task UpdateItemQuantityAsync(string userId, int productId, int quantity)
{
    var cart = await GetCartByUserIdAsync(userId);
    var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

    if (item != null)
    {
        if (quantity <= 0)
        {
            cart.Items.Remove(item); // Remove item if quantity is 0 or less
        }
        else
        {
            item.Quantity = quantity;
        }

        await _context.SaveChangesAsync();
    }
}


    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}

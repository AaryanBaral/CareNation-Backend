// OrderItemRepository.cs
using backend.Data;
using backend.Interface.Repository;
using backend.Models;
using Microsoft.EntityFrameworkCore;
namespace backend.Repository;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly AppDbContext _context;

    public OrderItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddOrderItemsAsync(List<OrderItem> items)
    {
        await _context.OrderItems.AddRangeAsync(items);
        await _context.SaveChangesAsync();
    }

    public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
    {
        return await _context.OrderItems
            .Include(oi => oi.Product)
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();
    }
}

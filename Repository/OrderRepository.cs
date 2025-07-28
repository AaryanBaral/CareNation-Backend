// OrderRepository.cs
using backend.Data;
using backend.Interface.Repository;
using backend.Models;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    public OrderRepository(AppDbContext context) => _context = context;

    public async Task CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        
        await _context.SaveChangesAsync();
    }
}

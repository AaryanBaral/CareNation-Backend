using backend.Data;
using backend.Interface.Repository;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Order>> SearchOrdersAsync(
            string? userId = null,
            OrderStatus status = OrderStatus.Pending,
            bool highestSaleOnly = false,
            DateTime? from = null,
            DateTime? to = null,
            int skip = 0,
            int take = 20)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)   // <— add this
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(o => o.UserId == userId);

            query = query.Where(o => o.Status == status);

            if (from.HasValue) query = query.Where(o => o.OrderDate >= from.Value);
            if (to.HasValue) query = query.Where(o => o.OrderDate <= to.Value);

            if (highestSaleOnly)
                return await query.OrderByDescending(o => o.TotalAmount).Take(1).ToListAsync();

            return await query.OrderByDescending(o => o.OrderDate)
                              .Skip(skip).Take(take).ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)   // <— add this
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)   // <— add this
                .ToListAsync();
        }
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null) return false;

            _context.Orders.Remove(order);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> ApproveOrderAsync(int orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.Pending) return false;

            order.Status = OrderStatus.Delivered;
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }
        //reject order code
        public async Task<bool> RejectOrderAsync(int orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.Pending) return false;

            order.Status = OrderStatus.Cancelled;
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)   // <— add this
                .ToListAsync();
        }

        public async Task<int> GetTotalOrdersCountAsync()
        {
            return await _context.Orders.CountAsync();
        }


        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)   // <— add this
                .ToListAsync();
        }
    }
}

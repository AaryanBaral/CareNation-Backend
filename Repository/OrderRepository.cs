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

        /// <summary>
        /// Search orders with filters and pagination
        /// </summary>
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
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(o => o.UserId == userId);

                query = query.Where(o => o.Status == status);

            if (from.HasValue)
                query = query.Where(o => o.OrderDate >= from.Value);

            if (to.HasValue)
                query = query.Where(o => o.OrderDate <= to.Value);

            if (highestSaleOnly)
                return await query
                    .OrderByDescending(o => o.TotalAmount)
                    .Take(1)
                    .ToListAsync();

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}

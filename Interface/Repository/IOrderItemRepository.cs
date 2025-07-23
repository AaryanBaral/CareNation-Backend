

using backend.Models;

namespace backend.Interface.Repository
{
    public interface IOrderItemRepository
    {
    Task AddOrderItemsAsync(List<OrderItem> items);
    Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
    }
}
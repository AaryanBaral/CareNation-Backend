
using backend.Dto;
using backend.Models;

namespace backend.Interface.Service
{
    public interface IOrderService
    {
        Task<int> PlaceOrderAsync(string userId);
        Task<bool> ApproveOrderAsync(int orderId);
        Task<bool> RejectOrderAsync(int orderId);
        Task<bool> DeleteOrderAsync(int orderId);

        // Queries (DTO-only)
        Task<OrderReadDto?> GetOrderByIdAsync(int orderId);
        Task<List<OrderReadDto>> GetOrdersByUserIdAsync(string userId);
        Task<List<OrderReadDto>> GetAllOrdersAsync();
        Task<List<OrderReadDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<int> GetTotalOrdersCountAsync();

        // Search (server-paged)
        Task<List<OrderReadDto>> SearchOrdersAsync(
            string? userId = null,
            OrderStatus status = OrderStatus.Pending,
            bool highestSaleOnly = false,
            DateTime? from = null,
            DateTime? to = null,
            int skip = 0,
            int take = 20
        );
    }
}
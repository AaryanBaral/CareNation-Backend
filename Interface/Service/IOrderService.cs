
using backend.Dto;
using backend.Models;

namespace backend.Interface.Service
{
    public interface IOrderService
    {
        Task<int> PlaceOrderAsync(string userId);

        Task<List<OrderReadDto>> SearchOrdersAsync(
            string? userId = null,
            OrderStatus status = OrderStatus.Pending,
            bool highestSaleOnly = false,
            DateTime? from = null,
            DateTime? to = null,
            int skip = 0,
            int take = 20
        );

        Task<Order?> GetOrderByIdAsync(int orderId);

        Task<List<Order>> GetOrdersByUserIdAsync(string userId);

        Task<bool> DeleteOrderAsync(int orderId);

        Task<bool> ApproveOrderAsync(int orderId);

        Task<bool> RejectOrderAsync(int orderId);

        Task<List<Order>> GetAllOrdersAsync();

        Task<int> GetTotalOrdersCountAsync();

        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
    }
}
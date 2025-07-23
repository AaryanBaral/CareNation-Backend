

using backend.Dto;

namespace backend.Interface.Service
{
    public interface IOrderItemService
    {
        Task<List<OrderItemReadDto>> GetItemsByOrderIdAsync(int orderId);
    }
}
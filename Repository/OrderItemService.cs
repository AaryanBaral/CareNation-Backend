using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Models;

namespace backend.Service
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _repository;

        public OrderItemService(IOrderItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<OrderItemReadDto>> GetItemsByOrderIdAsync(int orderId)
        {
            var items = await _repository.GetOrderItemsByOrderIdAsync(orderId);

            return items.Select(item => new OrderItemReadDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                OrderId = item.OrderId,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList();
        }
    }
}

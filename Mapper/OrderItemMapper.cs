using backend.Models;
using backend.Dto;

namespace backend.Mapper
{
    public static class OrderItemMapper
    {
        public static OrderItem ToOrderItem(this OrderItemCreateDto dto)
        {
            return new OrderItem
            {
                OrderId = dto.OrderId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Price = dto.Price
            };
        }

        public static OrderItem ToOrderItem(this OrderItemUpdateDto dto)
        {
            return new OrderItem
            {
                Id = dto.Id,
                OrderId = dto.OrderId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Price = dto.Price
            };
        }

        public static OrderItemReadDto ToReadDto(this OrderItem item)
        {
            return new OrderItemReadDto
            {
                Id = item.Id,
                OrderId = item.OrderId,
                ProductName = item.Product.Title,
                Quantity = item.Quantity,
                Price = item.Price
            };
        }

        public static void UpdateOrderItem(this OrderItem item, OrderItemUpdateDto dto)
        {
            item.OrderId = dto.OrderId;
            item.ProductId = dto.ProductId;
            item.Quantity = dto.Quantity;
            item.Price = dto.Price;
        }
    }
}

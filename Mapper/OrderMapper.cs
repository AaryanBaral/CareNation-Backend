using backend.Dto;
using backend.Models;

namespace backend.Mapper;

public static class OrderMapper
{
    public static OrderReadDto ToDto(this Order order)
    {
        return new OrderReadDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            Items = order.Items.Select(i => new OrderItemReadDto
            {
                Id= i.Id,
                ProductName = i.Product.Title,
                OrderId = order.Id,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };
    }
}

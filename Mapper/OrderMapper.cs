using backend.Dto;
using backend.Models;

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
            Items = (order.Items ?? new List<OrderItem>())
                .Select(i => new OrderItemReadDto
                {
                    Id = i.Id,
                    ProductName = i.Product?.Title ?? "(unknown product)",
                    OrderId = order.Id,
                    Quantity = i.Quantity,
                    Price = i.Price
                })
                .ToList()
        };
    }
}

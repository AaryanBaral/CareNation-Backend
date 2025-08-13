using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Mapper;
using backend.Models;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ICartRepository _cartRepo;
    private readonly IProductRepository _productRepo;
    private readonly IDistributorRepository _distributorRepository;


    public OrderService(IOrderRepository orderRepo, ICartRepository cartRepo, IProductRepository productRepo, IDistributorRepository distributorRepository)
    {
        _orderRepo = orderRepo;
        _cartRepo = cartRepo;
        _productRepo = productRepo;
        _distributorRepository = distributorRepository;
    }

    public async Task<int> PlaceOrderAsync(string userId)
    {
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);

        if (!cart.Items.Any()) throw new InvalidOperationException("Cart is empty.");

        decimal total = 0;
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        foreach (var item in cart.Items)
        {
            var product = await _productRepo.GetProductById(item.ProductId)
                          ?? throw new Exception("Product not found");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for {product.Title}");

            product.StockQuantity -= item.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = product.RetailPrice,
            };

            order.Items.Add(orderItem);
            total += item.Quantity * product.RetailPrice;
        }

        order.TotalAmount = total;

        await _orderRepo.CreateOrderAsync(order);
        await _cartRepo.ClearCartAsync(userId);

        return order.Id;
    }
    public async Task<List<OrderReadDto>> SearchOrdersAsync(
    string? userId = null,
    OrderStatus status = OrderStatus.Pending,
    bool highestSaleOnly = false,
    DateTime? from = null,
    DateTime? to = null,
    int skip = 0,
    int take = 20)
    {
        var orders = await _orderRepo.SearchOrdersAsync(userId, status, highestSaleOnly, from, to, skip, take);
        return [.. orders.Select(o => o.ToDto())];
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _orderRepo.GetOrderByIdAsync(orderId);
    }

    public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
    {
        return await _orderRepo.GetOrdersByUserIdAsync(userId);
    }

    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        return await _orderRepo.DeleteOrderAsync(orderId);
    }

    public async Task<bool> ApproveOrderAsync(int orderId)
    {
        await _orderRepo.ApproveOrderAsync(orderId);
        var order = await _orderRepo.GetOrderByIdAsync(orderId);
        if (order == null) return false;

        var userId = order.UserId;
        // Process commission for the distributor
        await _distributorRepository.ProcessCommissionOnSaleAsync(userId, order.TotalAmount);
        return true;
    }

    public async Task<bool> RejectOrderAsync(int orderId)
    {
        return await _orderRepo.RejectOrderAsync(orderId);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _orderRepo.GetAllOrdersAsync();
    }

    public async Task<int> GetTotalOrdersCountAsync()
    {
        return await _orderRepo.GetTotalOrdersCountAsync();
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _orderRepo.GetOrdersByStatusAsync(status);
    }
}

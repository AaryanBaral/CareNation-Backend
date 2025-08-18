using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Models;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ICartRepository _cartRepo;
    private readonly IProductRepository _productRepo;
    private readonly IDistributorRepository _distributorRepository;

    public OrderService(
        IOrderRepository orderRepo,
        ICartRepository cartRepo,
        IProductRepository productRepo,
        IDistributorRepository distributorRepository)
    {
        _orderRepo = orderRepo;
        _cartRepo = cartRepo;
        _productRepo = productRepo;
        _distributorRepository = distributorRepository;
    }

    public async Task<int> PlaceOrderAsync(string userId)
    {
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);
        if (cart == null || !cart.Items.Any())
            throw new InvalidOperationException("Cart is empty.");

        decimal total = 0m;
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

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = product.RetailPrice,
            });

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
        return orders.Select(o => o.ToDto()).ToList();
    }

    // FIX: map entity -> DTO
    public async Task<OrderReadDto?> GetOrderByIdAsync(int orderId)
    {
        var o = await _orderRepo.GetOrderByIdAsync(orderId);
        return o?.ToDto();
    }

    // FIX: map entity -> DTO
    public async Task<List<OrderReadDto>> GetOrdersByUserIdAsync(string userId)
    {
        var list = await _orderRepo.GetOrdersByUserIdAsync(userId);
        return list.Select(o => o.ToDto()).ToList();
    }

    public Task<bool> DeleteOrderAsync(int orderId) => _orderRepo.DeleteOrderAsync(orderId);

    // ---- Internal helpers on ENTITY (needed for PV + status checks) ----

    private static decimal CalculateOrderPV(Order order)
    {
        // ProductPoint may be null if Product not included; repo includes Product in Get* calls.
        return order.Items?.Sum(i => (decimal)i.Quantity * (i.Product?.ProductPoint ?? 0m)) ?? 0m;
    }

    private async Task<bool> IsRepurchaseAsync(Order order)
    {
        // We need previous *approved* orders for this user, earlier than current
        var all = await _orderRepo.GetOrdersByUserIdAsync(order.UserId);
        return all.Any(o => o.Status == OrderStatus.Delivered && o.OrderDate < order.OrderDate);
    }

    public async Task<bool> ApproveOrderAsync(int orderId)
    {
        // 1) Mark approved in repo
        var ok = await _orderRepo.ApproveOrderAsync(orderId);
        if (!ok) return false;

        // 2) Load the full order entity WITH Items+Product for PV/commissions
        var order = await _orderRepo.GetOrderByIdAsync(orderId);
        if (order == null) return false;

        var userId = order.UserId;

        // 3) Process binary/level commissions on sale amount
        await _distributorRepository.ProcessCommissionOnSaleAsync(userId, order.TotalAmount);

        // 4) If repurchase, distribute repurchase commissions off PV
        var isRepurchase = await IsRepurchaseAsync(order);
        if (isRepurchase)
        {
            var totalPV = CalculateOrderPV(order);
            if (totalPV > 0)
            {
                await _distributorRepository.ProcessRepurchaseAsync(userId, totalPV);
            }
        }

        return true;
    }

    public Task<bool> RejectOrderAsync(int orderId) => _orderRepo.RejectOrderAsync(orderId);

    // Map entity -> DTO for list endpoints
    public async Task<List<OrderReadDto>> GetAllOrdersAsync()
    {
        var list = await _orderRepo.GetAllOrdersAsync();
        return [.. list.Select(o => o.ToDto())];
    }

    public Task<int> GetTotalOrdersCountAsync() => _orderRepo.GetTotalOrdersCountAsync();

    public async Task<List<OrderReadDto>> GetOrdersByStatusAsync(OrderStatus status)
    {
        var list = await _orderRepo.GetOrdersByStatusAsync(status);
        return list.Select(o => o.ToDto()).ToList();
    }
}

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
                Price = product.Price,
            };

            order.Items.Add(orderItem);
            total += item.Quantity * product.Price;
        }

        order.TotalAmount = total;

        var distributor = await _distributorRepository.GetDistributorByIdAsync(userId);
        if (distributor is not null)
        {
            var comission = total * (decimal)0.1;
            await _distributorRepository.Addcommitsion(comission, distributor.ReferalId!);
        }


        await _orderRepo.CreateOrderAsync(order);
        await _cartRepo.ClearCartAsync(userId);
        await _distributorRepository.ProcessCommissionOnSaleAsync(userId, order.TotalAmount);

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
}

// OrderController.cs
using System.Security.Claims;
using backend.Dto;
using backend.Interface.Service;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(AuthenticationSchemes = "Bearer")]
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // POST api/order/place
    [HttpPost("place")]
    public async Task<ActionResult<SuccessResponseDto<string>>> PlaceOrder()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Login required");

        var orderId = await _orderService.PlaceOrderAsync(userId);
        return Ok(new SuccessResponseDto<string> { Data = $"Order #{orderId} placed successfully." });
    }

    // GET api/order/{orderId}
    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<SuccessResponseDto<Order?>>> GetById([FromRoute] int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        return Ok(new SuccessResponseDto<OrderReadDto?> { Data = order });
    }

    // GET api/order/my
    [HttpGet("my")]
    public async Task<ActionResult<SuccessResponseDto<List<Order>>>> GetMyOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Login required");

        var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        return Ok(new SuccessResponseDto<List<OrderReadDto>> { Data = orders });
    }

    // DELETE api/order/{orderId}
    [HttpDelete("{orderId:int}")]
    public async Task<ActionResult<SuccessResponseDto<string>>> Delete([FromRoute] int orderId)
    {
        var ok = await _orderService.DeleteOrderAsync(orderId);
        if (!ok) throw new KeyNotFoundException("Order not found or could not be deleted.");

        return Ok(new SuccessResponseDto<string> { Data = $"Order #{orderId} deleted." });
    }

    // POST api/order/approve/{orderId}
    [HttpPost("approve/{orderId:int}")]
    public async Task<ActionResult<SuccessResponseDto<string>>> Approve([FromRoute] int orderId)
    {
        var ok = await _orderService.ApproveOrderAsync(orderId);
        if (!ok) throw new KeyNotFoundException("Order not found or already processed.");

        return Ok(new SuccessResponseDto<string> { Data = $"Order #{orderId} approved." });
    }

    // POST api/order/reject/{orderId}
    [HttpPost("reject/{orderId:int}")]
    public async Task<ActionResult<SuccessResponseDto<string>>> Reject([FromRoute] int orderId)
    {
        var ok = await _orderService.RejectOrderAsync(orderId);
        if (!ok) throw new KeyNotFoundException("Order not found or already processed.");

        return Ok(new SuccessResponseDto<string> { Data = $"Order #{orderId} rejected." });
    }

    // GET api/order/search
    // Example: /api/order/search?userId=123&status=Approved&highestSaleOnly=true&from=2025-08-01&to=2025-08-11&skip=0&take=20
    [HttpGet("search")]
    public async Task<ActionResult<SuccessResponseDto<List<OrderReadDto>>>> Search(
        [FromQuery] string? userId = null,
        [FromQuery] OrderStatus status = OrderStatus.Pending,
        [FromQuery] bool highestSaleOnly = false,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var results = await _orderService.SearchOrdersAsync(userId, status, highestSaleOnly, from, to, skip, take);
        return Ok(new SuccessResponseDto<List<OrderReadDto>> { Data = results });
    }

    // GET api/order/all
    [HttpGet("all")]
    public async Task<ActionResult<SuccessResponseDto<List<Order>>>> GetAll()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(new SuccessResponseDto<List<OrderReadDto>> { Data = orders });
    }

    // GET api/order/count
    [HttpGet("count")]
    public async Task<ActionResult<SuccessResponseDto<int>>> GetCount()
    {
        var count = await _orderService.GetTotalOrdersCountAsync();
        return Ok(new SuccessResponseDto<int> { Data = count });
    }

    // GET api/order/status/{status}
    // Example: /api/order/status/Approved
    [HttpGet("status/{status}")]
    public async Task<ActionResult<SuccessResponseDto<List<Order>>>> GetByStatus([FromRoute] OrderStatus status)
    {
        var orders = await _orderService.GetOrdersByStatusAsync(status);
        return Ok(new SuccessResponseDto<List<OrderReadDto>> { Data = orders });
    }
}

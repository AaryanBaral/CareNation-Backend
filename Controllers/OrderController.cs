// OrderController.cs
using backend.Dto;
using backend.Interface.Service;
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

    [HttpPost("place")]
    public async Task<ActionResult<SuccessResponseDto<string>>> PlaceOrder()
    {
        var userId = User.FindFirst("Id")?.Value 
                     ?? throw new UnauthorizedAccessException("Login required");
        var orderId = await _orderService.PlaceOrderAsync(userId);
        return Ok(new SuccessResponseDto<string> { Data = $"Order #{orderId} placed successfully." });
    }
}

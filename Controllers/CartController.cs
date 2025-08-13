using System.Security.Claims;
using System.Text.Json;
using backend.Dto;
using backend.Interface.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Authorize(AuthenticationSchemes = "Bearer")]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _service;

    public CartController(ICartService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<SuccessResponseDto<CartReadDto>>> GetCart()
    {
        string userId = (User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new UnauthorizedAccessException("Please login to view this");
        var cart = await _service.GetUserCartAsync(userId);
        return Ok(new SuccessResponseDto<CartReadDto> { Data = cart });
    }


    [HttpPost("add")]
    public async Task<ActionResult<SuccessResponseDto
    <string>>> AddToCart([FromBody] CartItemDto item)
    {
        string userId = (User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new UnauthorizedAccessException("Please login to view this");
        await _service.AddToCartAsync(userId, item);
        return Ok(new SuccessResponseDto<string> { Data = "Item added to cart" });
    }

    [HttpDelete("{productId}")]
    public async Task<ActionResult<SuccessResponseDto<string>>> RemoveItem(int productId)
    {
        string userId = (User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new UnauthorizedAccessException("Please login to view this");
        await _service.RemoveFromCartAsync(userId, productId);
        return Ok(new SuccessResponseDto<string> { Data = "Item removed from cart" });
    }

    [HttpDelete("clear")]
    public async Task<ActionResult<SuccessResponseDto<string>>> ClearCart()
    {
        string userId = (User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new UnauthorizedAccessException("Please login to view this");
        await _service.ClearCartAsync(userId);
        return Ok(new SuccessResponseDto<string> { Data = "Cart cleared" });
    }
    [HttpPut("update-quantity")]
public async Task<ActionResult<SuccessResponseDto<string>>> UpdateItemQuantity([FromBody] UpdateQuantityDto dto)
{
    string userId = (User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new UnauthorizedAccessException("Please login to update cart");

    await _service.UpdateItemQuantityAsync(userId, dto.ProductId, dto.Quantity);

    return Ok(new SuccessResponseDto<string> { Data = "Item quantity updated" });
}

}

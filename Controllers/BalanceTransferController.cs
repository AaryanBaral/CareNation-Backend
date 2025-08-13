using System.Security.Claims;
using backend.Dto;
using backend.Interface.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(AuthenticationSchemes = "Bearer")]
[ApiController]
[Route("api/balance-transfer")]
public class BalanceTransferController : ControllerBase
{
    private readonly IBalanceTransferService _service;

    public BalanceTransferController(IBalanceTransferService service)
    {
        _service = service;
    }

    // Helper to get current user ID from token
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
    }

    /// <summary>
    /// Distributor initiates a balance transfer
    /// </summary>
    [HttpPost]
    // [Authorize(Roles = "Distributor")]
    public async Task<IActionResult> TransferBalance([FromBody] BalanceTransferDto dto)
    {
        try
        {
            var senderId = GetUserId();
            var result = await _service.TransferBalanceAsync(senderId, dto);
            return result ? Ok("Transfer successful.") : BadRequest("Transfer failed.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Admin or Superadmin views all balance transfers
    /// </summary>
    [HttpGet("all")]
    // [Authorize(Roles = "Admin,Superadmin")]
    public async Task<IActionResult> GetAllTransfers()
    {
        var transfers = await _service.GetAllTransfersAsync();
        return Ok(transfers);
    }

    /// <summary>
    /// Distributor views their own sent/received transfers
    /// </summary>
    [HttpGet("my")]
    // [Authorize(Roles = "Distributor")]
    public async Task<IActionResult> GetMyTransfers()
    {
        var userId = GetUserId();
        var transfers = await _service.GetTransfersByUserIdAsync(userId);
        return Ok(transfers);
    }
}

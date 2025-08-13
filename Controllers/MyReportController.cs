using System.Security.Claims;
using backend.Dto;
using backend.Interface.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = "SensitiveAction")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Route("api/my-reports")]
public class MyReportsController : ControllerBase
{
    private readonly IReportService _service;

    public MyReportsController(IReportService service)
    {
        _service = service;
    }

    private string GetUserId() => (User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new UnauthorizedAccessException("Please login to Viwe Reports");

    [HttpGet("personal-earnings")]
    public async Task<IActionResult> GetMyPersonalEarnings([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = GetUserId();
        var report = await _service.GetPersonalEarningsAsync(userId, from, to);
        if (report == null)
            return NotFound("Earnings report not found.");
        return Ok(report);
    }

    [HttpGet("commission-payouts")]
    public async Task<IActionResult> GetMyCommissionPayouts([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status = null)
    {
        var userId = GetUserId();
        var report = await _service.GetCommissionPayoutsByDistributorAsync(userId, from, to, status);
        return Ok(report);
    }

    [HttpGet("commission-payout-summary")]
    public async Task<IActionResult> GetMyCommissionPayoutSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = GetUserId();
        var summary = await _service.GetCommissionPayoutSummaryByDistributorAsync(userId, from, to);
        return Ok(summary);
    }

    [HttpGet("withdrawal-requests")]
    public async Task<IActionResult> GetMyWithdrawalRequests([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status = null)
    {
        var userId = GetUserId();
        var report = await _service.GetWithdrawalRequestsByDistributorAsync(userId, from, to, status);
        return Ok(report);
    }

    [HttpGet("withdrawal-transactions")]
    public async Task<IActionResult> GetMyWithdrawalTransactions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = GetUserId();
        var result = await _service.GetWithdrawalTransactionsByDistributorAsync(userId, from, to);
        return Ok(result);
    }

    [HttpGet("full-transactions")]
    public async Task<IActionResult> GetMyFullTransactionalReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = GetUserId();
        var result = await _service.GetFullTransactionalReportAsync(userId, from, to);
        return Ok(result);
    }
    [HttpGet("team-sales-by-time")]
    public async Task<ActionResult<List<SalesByTimeDto>>> GetMyTeamSalesByTime(
    [FromQuery] DateTime? from = null,
    [FromQuery] DateTime? to = null,
    [FromQuery] string period = "day")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Login required");

        var result = await _service.GetSalesByTimeForDistributorAsync(userId, from, to, period);
        return Ok(result);
    }
}

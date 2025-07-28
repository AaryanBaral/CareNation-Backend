using backend.Interface.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    [HttpGet("sales-by-product")]
    public async Task<IActionResult> GetSalesByProduct([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _service.GetSalesByProductAsync(from, to));

    [HttpGet("sales-by-category")]
    public async Task<IActionResult> GetSalesByCategory([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _service.GetSalesByCategoryAsync(from, to));

    [HttpGet("sales-by-time")]
    public async Task<IActionResult> GetSalesByTime([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string period = "day")
        => Ok(await _service.GetSalesByTimeAsync(from, to, period));

    [HttpGet("distributor-account-balances")]
    public async Task<IActionResult> GetDistributorAccountBalances()
        => Ok(await _service.GetDistributorAccountBalancesAsync());

    [HttpGet("product-stock-report")]
    public async Task<IActionResult> GetProductStockReport([FromQuery] int reorderLevel = 10)
    => Ok(await _service.GetProductStockReportAsync(reorderLevel));

    [HttpGet("personal-earnings")]
    public async Task<IActionResult> GetPersonalEarnings([FromQuery] string distributorId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var report = await _service.GetPersonalEarningsAsync(distributorId, from, to);
        if (report == null)
            return NotFound("Distributor not found.");
        return Ok(report);
    }

    [HttpGet("distributor-performance")]
    public async Task<IActionResult> GetDistributorPerformance([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    => Ok(await _service.GetDistributorPerformanceReportAsync(from, to));

    [HttpGet("user-growth")]
    public async Task<IActionResult> GetUserGrowth([FromQuery] string period = "month", [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    => Ok(await _service.GetUserGrowthReportAsync(period, from, to));
    [HttpGet("withdrawal-requests")]
    public async Task<IActionResult> GetWithdrawalRequests(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] string? status = null)
    {
        var report = await _service.GetWithdrawalRequestsAsync(from, to, status);
        return Ok(report);
    }

    [HttpGet("commission-payouts")]
    public async Task<IActionResult> GetCommissionPayouts(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] string? status = null)
    {
        var report = await _service.GetCommissionPayoutsAsync(from, to, status);
        return Ok(report);
    }
    [HttpGet("commission-payout-summary")]
    public async Task<IActionResult> GetCommissionPayoutSummary(
    [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var summary = await _service.GetCommissionPayoutSummaryAsync(from, to);
        return Ok(summary);
    }
    [HttpGet("withdrawal-transactions")]
    public async Task<IActionResult> GetWithdrawalTransactions(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _service.GetWithdrawalTransactionsAsync(from, to);
        return Ok(result);
    }
    [HttpGet("full-transactional-report")]
public async Task<IActionResult> GetFullTransactionalReport(
    [FromQuery] string? userId = null,
    [FromQuery] DateTime? from = null,
    [FromQuery] DateTime? to = null)
{
    
    var result = await _service.GetFullTransactionalReportAsync(userId, from, to);
    return Ok(result);
}




}

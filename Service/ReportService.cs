using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;

namespace backend.Service;

public class ReportService : IReportService
{
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<SalesByProductDto>> GetSalesByProductAsync(DateTime? from = null, DateTime? to = null)
        => await _repo.GetSalesByProductAsync(from, to);

    public async Task<List<SalesByCategoryDto>> GetSalesByCategoryAsync(DateTime? from = null, DateTime? to = null)
        => await _repo.GetSalesByCategoryAsync(from, to);

    public async Task<List<SalesByTimeDto>> GetSalesByTimeAsync(DateTime? from = null, DateTime? to = null, string period = "day")
        => await _repo.GetSalesByTimeAsync(from, to, period);
    public async Task<List<DistributorAccountBalanceDto>> GetDistributorAccountBalancesAsync()
       => await _repo.GetDistributorAccountBalancesAsync();
    public async Task<List<ProductStockReportDto>> GetProductStockReportAsync(int reorderLevel = 10)
        => await _repo.GetProductStockReportAsync(reorderLevel);
    public async Task<PersonalEarningsDto?> GetPersonalEarningsAsync(string distributorId, DateTime? from = null, DateTime? to = null)
        => await _repo.GetPersonalEarningsAsync(distributorId, from, to);
    public async Task<List<DistributorPerformanceDto>> GetDistributorPerformanceReportAsync(DateTime? from = null, DateTime? to = null)
        => await _repo.GetDistributorPerformanceReportAsync(from, to);
    public async Task<List<UserGrowthDto>> GetUserGrowthReportAsync(string period = "month", DateTime? from = null, DateTime? to = null)
    => await _repo.GetUserGrowthReportAsync(period, from, to);
    public async Task<List<WithdrawalRequestDto>> GetWithdrawalRequestsAsync(DateTime? from = null, DateTime? to = null, string? status = null)
    => await _repo.GetWithdrawalRequestsAsync(from, to, status);
    public async Task<List<CommissionPayoutDto>> GetCommissionPayoutsAsync(DateTime? from = null, DateTime? to = null, string? status = null)
    => await _repo.GetCommissionPayoutsAsync(from, to, status);
    public async Task<List<CommissionPayoutSummaryDto>> GetCommissionPayoutSummaryAsync(DateTime? from = null, DateTime? to = null)
    => await _repo.GetCommissionPayoutSummaryAsync(from, to);
    public async Task<List<WithdrawalTransactionDto>> GetWithdrawalTransactionsAsync(DateTime? from = null, DateTime? to = null)
    => await _repo.GetWithdrawalTransactionsAsync(from, to);

    public async Task<List<FullTransactionDto>> GetFullTransactionalReportAsync(
        string? userId = null, DateTime? from = null, DateTime? to = null)
        => await _repo.GetFullTransactionalReportAsync(userId, from, to);
        



}

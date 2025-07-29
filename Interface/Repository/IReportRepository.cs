using backend.Dto;

namespace backend.Interface.Repository;

public interface IReportRepository
{
    Task<List<SalesByProductDto>> GetSalesByProductAsync(DateTime? from = null, DateTime? to = null);
    Task<List<SalesByCategoryDto>> GetSalesByCategoryAsync(DateTime? from = null, DateTime? to = null);
    Task<List<SalesByTimeDto>> GetSalesByTimeAsync(DateTime? from = null, DateTime? to = null, string period = "day");
    Task<List<DistributorAccountBalanceDto>> GetDistributorAccountBalancesAsync();
    Task<List<ProductStockReportDto>> GetProductStockReportAsync(int reorderLevel = 10);
    Task<PersonalEarningsDto?> GetPersonalEarningsAsync(string distributorId, DateTime? from = null, DateTime? to = null);
    Task<List<DistributorPerformanceDto>> GetDistributorPerformanceReportAsync(DateTime? from = null, DateTime? to = null);
    Task<List<UserGrowthDto>> GetUserGrowthReportAsync(string period = "month", DateTime? from = null, DateTime? to = null);
    Task<List<WithdrawalRequestDto>> GetWithdrawalRequestsAsync(DateTime? from = null, DateTime? to = null, string? status = null);
    Task<List<CommissionPayoutDto>> GetCommissionPayoutsAsync(DateTime? from = null, DateTime? to = null, string? status = null);
    Task<List<CommissionPayoutSummaryDto>> GetCommissionPayoutSummaryAsync(DateTime? from = null, DateTime? to = null);
    Task<List<WithdrawalTransactionDto>> GetWithdrawalTransactionsAsync(DateTime? from = null, DateTime? to = null);
    Task<List<FullTransactionDto>> GetFullTransactionalReportAsync(
    string? userId = null, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<CommissionPayoutDto>> GetCommissionPayoutsByDistributorAsync(string userId, DateTime? from, DateTime? to, string? status);
    Task<CommissionPayoutSummaryDto> GetCommissionPayoutSummaryByDistributorAsync(string userId, DateTime? from, DateTime? to);
    Task<IEnumerable<WithdrawalRequestDto>> GetWithdrawalRequestsByDistributorAsync(string userId, DateTime? from, DateTime? to, string? status);
    Task<IEnumerable<WithdrawalTransactionDto>> GetWithdrawalTransactionsByDistributorAsync(string userId, DateTime? from, DateTime? to);
        



             
}

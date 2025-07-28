using backend.Models;

namespace backend.Interface.Service
{
    public interface ICommissionPayoutService
    {
        Task<List<CommissionPayout>> GetAllAsync();
        Task<CommissionPayout?> GetByIdAsync(int id);
        Task<List<CommissionPayout>> GetByUserIdAsync(string userId);
        Task AddAsync(CommissionPayout payout);
        Task<bool> UpdateStatusAsync(int id, string status, string? remarks);
        Task<bool> DeleteAsync(int id);
    }
}

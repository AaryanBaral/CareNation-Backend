using backend.Models;

namespace backend.Interface.Repository
{
    public interface ICommissionPayoutRepository
    {
        Task<List<CommissionPayout>> GetAllAsync();
        Task<CommissionPayout?> GetByIdAsync(int id);
        Task<List<CommissionPayout>> GetByUserIdAsync(string userId);
        Task AddAsync(CommissionPayout payout);
        Task<bool> UpdateStatusAsync(int id, string newStatus, string? remarks = null);
        Task<bool> DeleteAsync(int id);
    }
}

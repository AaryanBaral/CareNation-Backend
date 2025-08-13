using backend.Models;

namespace backend.Interface.Repository;
public interface IWithdrawalRequestRepository
{
    Task<List<WithdrawalRequest>> GetAllAsync();
    Task<WithdrawalRequest?> GetByIdAsync(int id);
    Task<List<WithdrawalRequest>> SearchAsync(string? userIdOrName);
    Task CreateAsync(WithdrawalRequest request);
    Task UpdateAsync(WithdrawalRequest request);
    Task DeleteAsync(int id);
}
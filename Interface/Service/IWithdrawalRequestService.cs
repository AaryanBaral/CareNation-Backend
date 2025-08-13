

namespace backend.Interface.Service
{
    public interface IWithdrawalRequestService
    {
        Task<List<WithdrawalRequestDto>> GetAllAsync();
        Task<List<WithdrawalRequestDto>> SearchAsync(string keyword);
        Task<WithdrawalRequestDto?> GetByIdAsync(int id);
        Task<WithdrawalRequestDto> CreateAsync(string userId, decimal amount, string? remarks);
        Task<bool> ApproveAsync(int id, string remarks = "");
        Task<bool> RejectAsync(int id, string remarks = "");
    }
}

using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Models;

namespace backend.Service
{
    public class CommissionPayoutService:ICommissionPayoutService
    {
        private readonly ICommissionPayoutRepository _payoutRepository;

        public CommissionPayoutService(ICommissionPayoutRepository payoutRepository)
        {
            _payoutRepository = payoutRepository;
        }

        public Task<List<CommissionPayout>> GetAllAsync() => _payoutRepository.GetAllAsync();
        public Task<CommissionPayout?> GetByIdAsync(int id) => _payoutRepository.GetByIdAsync(id);
        public Task<List<CommissionPayout>> GetByUserIdAsync(string userId) => _payoutRepository.GetByUserIdAsync(userId);
        public Task AddAsync(CommissionPayout payout) => _payoutRepository.AddAsync(payout);
        public Task<bool> UpdateStatusAsync(int id, string status, string? remarks) => _payoutRepository.UpdateStatusAsync(id, status, remarks);
        public Task<bool> DeleteAsync(int id) => _payoutRepository.DeleteAsync(id);
    }
}

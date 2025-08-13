using backend.Data;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Service
{
    public class WithdrawalRequestService:IWithdrawalRequestService
    {
        private readonly IWithdrawalRequestRepository _repository;
        private readonly AppDbContext _context;

        public WithdrawalRequestService(IWithdrawalRequestRepository repository, AppDbContext context)
        {
            _repository = repository; 
            _context = context;
        }

        public async Task<List<WithdrawalRequestDto>> GetAllAsync()
        {
            var requests = await _repository.GetAllAsync();
            return requests.Select(r => r.ToWithdrawalRequestDto()).ToList();
        }

        public async Task<List<WithdrawalRequestDto>> SearchAsync(string keyword)
        {
            var requests = await _repository.SearchAsync(keyword);
            return requests.Select(r => r.ToWithdrawalRequestDto()).ToList();
        }

        public async Task<WithdrawalRequestDto?> GetByIdAsync(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            return request?.ToWithdrawalRequestDto();
        }

        public async Task<WithdrawalRequestDto> CreateAsync(string userId, decimal amount, string? remarks)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                       ?? throw new Exception("User not found");

            var entity = new WithdrawalRequest
            {
                UserId = userId,
                Amount = amount,
                Remarks = remarks
            };

            await _repository.CreateAsync(entity);
            return entity.ToWithdrawalRequestDto();
        }

        public async Task<bool> ApproveAsync(int id, string remarks = "")
        {
            var request = await _repository.GetByIdAsync(id);
            if (request == null || request.Status != "Pending") return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null || user.TotalWallet < request.Amount) return false;

            request.Status = "Approved";
            request.ProcessedDate = DateTime.UtcNow;
            request.Remarks = remarks;

            user.TotalWallet -= request.Amount;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int id, string remarks = "")
        {
            var request = await _repository.GetByIdAsync(id);
            if (request == null || request.Status != "Pending") return false;

            request.Status = "Rejected";
            request.ProcessedDate = DateTime.UtcNow;
            request.Remarks = remarks;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

using backend.Data;
using backend.Models;
using backend.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class WithdrawalRequestRepository : IWithdrawalRequestRepository
    {
        private readonly AppDbContext _context;

        public WithdrawalRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WithdrawalRequest>> GetAllAsync()
        {
            return await _context.WithdrawalRequests
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<WithdrawalRequest?> GetByIdAsync(int id)
        {
            return await _context.WithdrawalRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<WithdrawalRequest>> SearchAsync(string? userIdOrName)
        {
            var term = userIdOrName?.Trim();

            var query = _context.WithdrawalRequests
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(r =>
                       r.UserId == term
                    || (r.User.Email ?? "").Contains(term)
                    || (((r.User.FirstName ?? "") + " " + (r.User.MiddleName ?? "") + " " + (r.User.LastName ?? "")).Contains(term))
                );
            }

            return await query.ToListAsync();
        }

        public async Task CreateAsync(WithdrawalRequest request)
        {
            _context.WithdrawalRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WithdrawalRequest request)
        {
            _context.WithdrawalRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var request = await _context.WithdrawalRequests.FindAsync(id);
            if (request != null)
            {
                _context.WithdrawalRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }
    }
}

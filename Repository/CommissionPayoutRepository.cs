using backend.Data;
using backend.Interface.Repository;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class CommissionPayoutRepository : ICommissionPayoutRepository
    {
        private readonly AppDbContext _context;

        public CommissionPayoutRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CommissionPayout>> GetAllAsync()
        {
            return await _context.CommissionPayouts.Include(p => p.User).ToListAsync();
        }

        public async Task<CommissionPayout?> GetByIdAsync(int id)
        {
            return await _context.CommissionPayouts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<CommissionPayout>> GetByUserIdAsync(string userId)
        {
            return await _context.CommissionPayouts
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task AddAsync(CommissionPayout payout)
        {
            _context.CommissionPayouts.Add(payout);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string newStatus, string? remarks = null)
        {
            var payout = await _context.CommissionPayouts.FindAsync(id);
            if (payout == null) return false;

            payout.Status = newStatus;
            if (remarks != null)
                payout.Remarks = remarks;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payout = await _context.CommissionPayouts.FindAsync(id);
            if (payout == null) return false;

            _context.CommissionPayouts.Remove(payout);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

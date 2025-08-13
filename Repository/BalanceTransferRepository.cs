using backend.Data;
using backend.Interface.Repository;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository;
public class BalanceTransferRepository : IBalanceTransferRepository
{
    private readonly AppDbContext _context;

    public BalanceTransferRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<bool> TransferAsync(BalanceTransfer transfer)
    {
        var sender = await _context.Users.FindAsync(transfer.SenderId);
        var receiver = await _context.Users.FindAsync(transfer.ReceiverId);

        if (sender == null || receiver == null)
            throw new Exception("Sender or receiver not found.");

        if (transfer.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        if (sender.TotalWallet < transfer.Amount)
            throw new InvalidOperationException("Sender has insufficient balance.");

        // Perform wallet operations
        sender.TotalWallet -= transfer.Amount;
        receiver.TotalWallet += transfer.Amount;

        _context.BalanceTransfers.Add(transfer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<BalanceTransfer>> GetAllTransfersAsync()
    {
        return await _context.BalanceTransfers
            .Include(t => t.Sender)
            .Include(t => t.Receiver)
            .OrderByDescending(t => t.TransferDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BalanceTransfer>> GetTransfersByUserIdAsync(string userId)
    {
        return await _context.BalanceTransfers
            .Where(t => t.SenderId == userId || t.ReceiverId == userId)
            .Include(t => t.Sender)
            .Include(t => t.Receiver)
            .OrderByDescending(t => t.TransferDate)
            .ToListAsync();
    }
}

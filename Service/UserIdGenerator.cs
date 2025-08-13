using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Transactions;
using backend.Data; // for Transaction.Current

public class UserIdGenerator : IUserIdGenerator
{
    private readonly AppDbContext _db;
    public UserIdGenerator(AppDbContext db) => _db = db;

    public async Task<string> NextAsync()
    {
        IDbContextTransaction? startedTx = null;
        try
        {
            // If caller already has a transaction / TransactionScope, don't start another
            var hasAmbient = Transaction.Current != null;
            var hasDbTx = _db.Database.CurrentTransaction != null;

            if (!hasAmbient && !hasDbTx)
            {
                startedTx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            }

            // Ensure the counter row exists (handle concurrent first-creation)
            var row = await _db.SystemCounters.SingleOrDefaultAsync(x => x.Name == "UserId");
            if (row == null)
            {
                row = new SystemCounter { Name = "UserId", NextValue = 1 };
                _db.SystemCounters.Add(row);
                try
                {
                    await _db.SaveChangesAsync();
                }
                catch
                {
                    // Likely a concurrent insert hit the unique constraint; re-read
                    _db.Entry(row).State = EntityState.Detached;
                    row = await _db.SystemCounters.SingleAsync(x => x.Name == "UserId");
                }
            }

            var n = row.NextValue;      // current value to use
            row.NextValue = n + 1;      // increment for next time
            await _db.SaveChangesAsync();

            if (startedTx != null)
                await startedTx.CommitAsync();
            return $"CN{n:D6}";
        }
        catch
        {
            if (startedTx != null)
                await startedTx.RollbackAsync();
            throw;
        }
    }
}

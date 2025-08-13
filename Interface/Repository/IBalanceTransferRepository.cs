using backend.Models;

namespace backend.Interface.Repository;
public interface IBalanceTransferRepository
{
    Task<bool> TransferAsync(BalanceTransfer transfer);
    Task<IEnumerable<BalanceTransfer>> GetAllTransfersAsync();
    Task<IEnumerable<BalanceTransfer>> GetTransfersByUserIdAsync(string userId);
}

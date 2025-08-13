using backend.Dto;

namespace backend.Interface.Service;
public interface IBalanceTransferService
{
    Task<bool> TransferBalanceAsync(string senderId, BalanceTransferDto dto);
    Task<IEnumerable<BalanceTransferViewDto>> GetAllTransfersAsync();
    Task<IEnumerable<BalanceTransferViewDto>> GetTransfersByUserIdAsync(string userId);
}

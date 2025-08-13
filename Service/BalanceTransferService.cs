using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Mapper;
using System.Linq;

namespace backend.Service
{
    public class BalanceTransferService : IBalanceTransferService
    {
        private readonly IBalanceTransferRepository _repo;

        public BalanceTransferService(IBalanceTransferRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> TransferBalanceAsync(string senderId, BalanceTransferDto dto)
        {
            if (dto.ReceiverId == senderId)
                throw new InvalidOperationException("Reciever and sender cannot be the same");

            var transfer = BalanceTransferMapper.ToModel(dto, senderId);
            return await _repo.TransferAsync(transfer);
        }

        public async Task<IEnumerable<BalanceTransferViewDto>> GetAllTransfersAsync()
        {
            var transfers = await _repo.GetAllTransfersAsync();
            return transfers.Select(t => new BalanceTransferViewDto
            {
                Id = t.Id,
                SenderId = t.SenderId,
                SenderName = $"{t.Sender.FirstName} {(t.Sender.MiddleName ?? "")} {t.Sender.LastName}".Trim(),
                ReceiverId = t.ReceiverId,
                ReceiverName = $"{t.Receiver.FirstName} {(t.Receiver.MiddleName ?? "")} {t.Receiver.LastName}".Trim(),
                Amount = t.Amount,
                TransferDate = t.TransferDate,
                Remarks = t.Remarks
            });
        }

        public async Task<IEnumerable<BalanceTransferViewDto>> GetTransfersByUserIdAsync(string userId)
        {
            var transfers = await _repo.GetTransfersByUserIdAsync(userId);
            return transfers.Select(t => new BalanceTransferViewDto
            {
                Id = t.Id,
                SenderId = t.SenderId,
                SenderName = $"{t.Sender.FirstName} {(t.Sender.MiddleName ?? "")} {t.Sender.LastName}".Trim(),
                ReceiverId = t.ReceiverId,
                ReceiverName = $"{t.Receiver.FirstName} {(t.Receiver.MiddleName ?? "")} {t.Receiver.LastName}".Trim(),
                Amount = t.Amount,
                TransferDate = t.TransferDate,
                Remarks = t.Remarks
            });
        }
    }
}

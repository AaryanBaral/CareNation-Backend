using backend.Dto;
using backend.Models;

namespace backend.Mapper;

public static class BalanceTransferMapper
{
    public static BalanceTransfer ToModel(BalanceTransferDto dto, string senderId)
    {
        return new BalanceTransfer
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            Amount = dto.Amount,
            Remarks = dto.Remarks
        };
    }
}

namespace backend.Dto;

public class BalanceTransferDto
{
    public string ReceiverId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }
}
public class BalanceTransferViewDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string ReceiverId { get; set; } = "";
    public string ReceiverName { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; }
    public string? Remarks { get; set; }
}


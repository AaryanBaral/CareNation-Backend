public class FullTransactionDto
{
    public string TransactionType { get; set; } = ""; // "Sale", "CommissionPayout", "Withdrawal", etc.
    public int TransactionId { get; set; }
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string UserEmail { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Status { get; set; }
    public string? Remarks { get; set; }
}

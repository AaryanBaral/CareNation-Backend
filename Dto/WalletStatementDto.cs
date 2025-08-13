public class WalletTransactionDto
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = ""; // e.g. "Commission", "Transfer In", "Transfer Out", "Withdrawal"
    public decimal Amount { get; set; }    // +ve for credit, -ve for debit
    public string? Remarks { get; set; }
    public decimal BalanceAfter { get; set; } // Calculated in backend
}
public class WalletStatementDto
{
    public decimal WalletBalance { get; set; }
    public List<WalletTransactionDto> Transactions { get; set; } = new();
}

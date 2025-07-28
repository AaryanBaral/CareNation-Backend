public class DistributorAccountBalanceDto
{
    public string DistributorId { get; set; } = "";
    public string DistributorName { get; set; } = "";
    public string DistributorEmail { get; set; } = "";
    public decimal TotalWallet { get; set; }
    public decimal CommissionAmount { get; set; }
}

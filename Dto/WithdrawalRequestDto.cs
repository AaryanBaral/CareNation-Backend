public class WithdrawalRequestDto
{
    public int Id { get; set; }
    public string DistributorId { get; set; } = "";
    public string DistributorName { get; set; } = "";
    public string DistributorEmail { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = "";
    public DateTime? ProcessedDate { get; set; }
    public string? Remarks { get; set; }
}
public class WithdrawalTransactionDto
{
    public int Id { get; set; }
    public string DistributorId { get; set; } = "";
    public string DistributorName { get; set; } = "";
    public string DistributorEmail { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = "";
    public DateTime? ProcessedDate { get; set; }
    public string? Remarks { get; set; }
}


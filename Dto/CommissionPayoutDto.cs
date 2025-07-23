public class CommissionPayoutDto
{
    public int Id { get; set; }
    public string DistributorId { get; set; } = "";
    public string DistributorName { get; set; } = "";
    public string DistributorEmail { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime PayoutDate { get; set; }
    public string Status { get; set; } = "";
    public string? Remarks { get; set; }
}

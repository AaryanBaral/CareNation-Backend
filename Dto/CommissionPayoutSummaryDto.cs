public class CommissionPayoutSummaryDto
{
    public string DistributorId { get; set; } = "";
    public string DistributorName { get; set; } = "";
    public string DistributorEmail { get; set; } = "";
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalFailed { get; set; }
    public int TotalPayouts { get; set; }
}

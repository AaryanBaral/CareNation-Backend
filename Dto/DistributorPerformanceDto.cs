public class DistributorPerformanceDto
{
    public string DistributorId { get; set; } = "";
    public string DistributorName { get; set; } = "";
    public string DistributorEmail { get; set; } = "";
    public int DirectRecruits { get; set; }
    public int TotalDownline { get; set; }
    public decimal PersonalSales { get; set; }
    public decimal TeamSales { get; set; }
    public decimal TotalCommission { get; set; }
}

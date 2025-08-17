public class TeamSalesProgress
{
    public string UserId { get; set; } = default!;
        public bool IsDeleted { get; set; }
    public decimal LeftTeamSales { get; set; }     // cumulative
    public decimal RightTeamSales { get; set; }    // cumulative
    public decimal MatchedVolumeConsumed { get; set; } // how much of Matched already paid (prevents double pay)
}
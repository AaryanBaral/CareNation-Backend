public enum RewardFundType { Royalty, TravelFund, CarFund, HouseFund }

public class RewardPayout
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public DateTime PayoutDate { get; set; } = DateTime.UtcNow;

    public decimal MilestoneAmount { get; set; }     // e.g., 100000, 300000, ...
    public string RankLabel { get; set; } = default!; // e.g., "Executive"
    public string RewardItem { get; set; } = "";      // e.g., "CNI Bag"
    public decimal RoyaltyAmount { get; set; }        // money sent to wallet
    public decimal TravelFundAmount { get; set; }     // contributed to fund
    public decimal CarFundAmount { get; set; }
    public decimal HouseFundAmount { get; set; }
        public bool IsDeleted { get; set; }
}
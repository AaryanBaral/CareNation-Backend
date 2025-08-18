public enum PointsType
{
    RepurchaseLevel,   // 42% chain (level 0..10)
    RoyaltyFund,       // 20% pool
    TravelFund,        // 10% pool
    CarFund,           // 10% pool
    HouseFund,         // 10% pool
    CompanyShare       // 8% + unallocated slices
}

public class PointsTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = default!;
    public PointsType Type { get; set; }
    public decimal Points { get; set; }

    public string? Note { get; set; }
    public string? SourceUserId { get; set; }  // purchaser/upline who triggered it
    public int? Level { get; set; }            // level for RepurchaseLevel (0..10)

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
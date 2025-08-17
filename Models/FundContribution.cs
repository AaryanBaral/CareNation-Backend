using backend.Models;

public class FundContribution
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!; // FK to AspNetUsers
    public FundType Type { get; set; }             // Enum: Royalty, Travel, Car, House
    public decimal Amount { get; set; }
    public DateTime ContributionDate { get; set; }
    public string? Remarks { get; set; }
        public bool IsDeleted { get; set; }

    // Navigation
    public User User { get; set; } = default!;
}

public enum FundType
{
    Royalty = 1,
    Travel  = 2,
    Car     = 3,
    House   = 4
}

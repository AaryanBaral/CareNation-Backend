namespace backend.Models;

public class WithdrawalRequest
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public User User { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public DateTime? ProcessedDate { get; set; }
    public string? Remarks { get; set; }
        public bool IsDeleted { get; set; }
}

namespace backend.Models;
public class CommissionPayout
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public User User { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PayoutDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Paid, Failed, etc.
    public string? Remarks { get; set; }
        public bool IsDeleted { get; set; }
}

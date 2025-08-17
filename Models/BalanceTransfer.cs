using backend.Models;
namespace backend.Models;
public class BalanceTransfer
{
    public int Id { get; set; }

    public string SenderId { get; set; } = string.Empty;
    public User Sender { get; set; } = null!;

    public string ReceiverId { get; set; } = string.Empty;
    public User Receiver { get; set; }= null!;
    public bool IsDeleted { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }
}

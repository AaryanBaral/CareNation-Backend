

using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
        public enum NodePosition
    {
        Left,
        Right
    }
    public class User : IdentityUser
    {
        public required string Address { get; set; }
        public required string FullName { get; set; }
        public string? DOB { get; set; }
        public string? CitizenshipNo { get; set; }
        public string? AccountName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? ParentId { get; set; }
        public string? ReferalId { get; set; }
        public double CommisionAmmount { get; set; } = 0.0;
        public double TotalWallet { get; set; } = 0.0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public NodePosition? Position { get; set; } // Nullable: root has no position
    }
    
}
// Models/ImpersonationTicket.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class ImpersonationTicket
    {
        [Key]
        public string Code { get; set; } = Guid.NewGuid().ToString("N");
        [Required] public string AdminId { get; set; } = default!;
        [Required] public string TargetUserId { get; set; } = default!;
        public DateTimeOffset ExpiresAt { get; set; }
        public bool Used { get; set; } = false;
        public string? Reason { get; set; }
        public string? ReturnUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UsedAt { get; set; }
    }
}

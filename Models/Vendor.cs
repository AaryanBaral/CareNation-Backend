using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Vendor
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        public string? ContactPerson { get; set; }
            public bool IsDeleted { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        public string? TaxId { get; set; }

        public bool IsActive { get; set; } = true;

        // Optional: Vendor Products Navigation
        // public ICollection<Product> Products { get; set; }
    }
}

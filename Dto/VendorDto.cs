
namespace backend.Dto;
public class VendorCreateDto
{
    public string Name { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? TaxId { get; set; }
}

// VendorUpdateDto.cs
public class VendorUpdateDto
{
    public string Name { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? TaxId { get; set; }
    public bool IsActive { get; set; }
}

// VendorReadDto.cs
public class VendorReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? TaxId { get; set; }
    public bool IsActive { get; set; }
}

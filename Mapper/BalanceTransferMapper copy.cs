using backend.Dto;
using backend.Models;

public static class VendorMapper
{
    public static VendorReadDto ToReadDto(this Vendor v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        CompanyName = v.CompanyName,
        ContactPerson = v.ContactPerson,
        Phone = v.Phone,
        Email = v.Email,
        Address = v.Address,
        City = v.City,
        Country = v.Country,
        TaxId = v.TaxId,
        IsActive = v.IsActive
    };

    public static Vendor ToEntity(this VendorCreateDto dto) => new()
    {
        Name = dto.Name,
        CompanyName = dto.CompanyName,
        ContactPerson = dto.ContactPerson,
        Phone = dto.Phone,
        Email = dto.Email,
        Address = dto.Address,
        City = dto.City,
        Country = dto.Country,
        TaxId = dto.TaxId,
        IsActive = true
    };

    public static void UpdateEntity(this Vendor v, VendorUpdateDto dto)
    {
        v.Name = dto.Name;
        v.CompanyName = dto.CompanyName;
        v.ContactPerson = dto.ContactPerson;
        v.Phone = dto.Phone;
        v.Email = dto.Email;
        v.Address = dto.Address;
        v.City = dto.City;
        v.Country = dto.Country;
        v.TaxId = dto.TaxId;
        v.IsActive = dto.IsActive;
    }
}

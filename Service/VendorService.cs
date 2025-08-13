using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;

namespace backend.Service;
public class VendorService : IVendorService
{
    private readonly IVendorRepository _repo;

    public VendorService(IVendorRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<VendorReadDto>> GetAllVendorsAsync()
    { 
        var vendors = await _repo.GetAllAsync();
        return vendors.Select(v => v.ToReadDto()).ToList();
    }

    public async Task<VendorReadDto?> GetVendorByIdAsync(int id)
    {
        var vendor = await _repo.GetByIdAsync(id);
        return vendor?.ToReadDto();
    }

    public async Task<VendorReadDto> CreateVendorAsync(VendorCreateDto dto)
    {
        // Business validation
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Vendor name is required.");

        var entity = dto.ToEntity();
        await _repo.AddAsync(entity);
        return entity.ToReadDto();
    }

    public async Task<bool> UpdateVendorAsync(int id, VendorUpdateDto dto)
    {
        var vendor = await _repo.GetByIdAsync(id);
        if (vendor == null) return false;

        vendor.UpdateEntity(dto);
        await _repo.UpdateAsync(vendor);
        return true;
    }

    public async Task<bool> DeleteVendorAsync(int id)
    {
        var vendor = await _repo.GetByIdAsync(id);
        if (vendor == null) return false;
        await _repo.DeleteAsync(vendor);
        return true;
    }
}

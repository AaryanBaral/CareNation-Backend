using backend.Dto;

namespace backend.Interface.Service;
public interface IVendorService
{
    Task<List<VendorReadDto>> GetAllVendorsAsync();
    Task<VendorReadDto?> GetVendorByIdAsync(int id);
    Task<VendorReadDto> CreateVendorAsync(VendorCreateDto dto);
    Task<bool> UpdateVendorAsync(int id, VendorUpdateDto dto);
    Task<bool> DeleteVendorAsync(int id);
}

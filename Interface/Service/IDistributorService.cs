using backend.Dto;
using backend.Models;

namespace backend.Interface.Service
{
    public interface IDistributorService
    {
        Task<bool> CanBecomeDistributorAsync(string userId);
        Task<DistributorReadDto?> GetDistributorByIdAsync(string id);
        Task<List<DistributorReadDto>> GetAllDistributorsAsync();
        Task<bool> SignUpDistributorAsync(string userId, DistributorSignUpDto dto);
        Task<bool> UpdateDistributorAsync(string id, DistributorSignUpDto dto);
        Task<bool> DeleteDistributorAsync(string id);
        Task<DistributorLoginResponse> LoginDistributorAsync(DistributorLoginDto dto);
        Task<DistributorTreeDto?> GetUserTreeAsync(string userId);
    Task<List<DistributorReadDto>> GetPeopleIReferredAsync(string myUserId);
    Task<List<DistributorReadDto>> GetMyUplineAsync(string myUserId);
        
    }
}

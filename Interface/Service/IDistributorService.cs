using backend.Dto;
using backend.Models;

namespace backend.Interface.Service
{
    public interface IDistributorService
    {
        Task<bool> CanBecomeDistributorAsync(string userId);
        Task<DistributorReadDto?> GetDistributorByIdAsync(string id);
        Task<List<DistributorReadDto>> GetAllDistributorsAsync();
        Task<bool> SignUpDistributorAsync(string userId, DistributorSignUpDto dto, IFormFile citizenshipFile, IFormFile? profilePicture = null);
        Task<bool> UpdateDistributorAsync(string id, DistributorSignUpDto dto);
        Task<bool> DeleteDistributorAsync(string id);
        Task<DistributorLoginResponse> LoginDistributorAsync(DistributorLoginDto dto);
        Task<DistributorTreeDto?> GetUserTreeAsync(string userId);
        Task<List<DistributorReadDto>> GetPeopleIReferredAsync(string myUserId);
        Task<List<DistributorReadDto>> GetMyUplineAsync(string myUserId);
        Task<int> GetTotalReferralsAsync(string userId);
        Task<int> GetTotalDownlineAsync(string userId);
        Task ChangeParentAsync(string userId, string newParentId, string childId);
        Task<WalletStatementDto> GetWalletStatementAsync(string userId);
                Task<List<DownlineUserDto>> GetDownlineAsync(string userId);

        
    }
}

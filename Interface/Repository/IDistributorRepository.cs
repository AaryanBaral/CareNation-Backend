using backend.Dto;
using backend.Models;
namespace backend.Interface.Repository;

public interface IDistributorRepository
{
    Task<bool> CanBecomeDistributorAsync(string userId);
    Task<User?> LoginDistributorAsync(string email, string password);
    Task<bool> SignUpDistributorAsync(User user);
    Task<User?> GetDistributorByIdAsync(string id);
    Task<List<User>> GetAllDistributorsAsync();
    Task<bool> UpdateDistributorAsync(User user);
    Task<bool> DeleteDistributorAsync(string id);
    Task Addcommitsion(decimal commision, string userId);
    Task<DistributorTreeDto?> GetUserTreeAsync(string userId);
    Task<List<User>> GetPeopleIReferredAsync(string myUserId);
    Task<List<User>> GetMyUplineAsync(string myUserId);
    Task ProcessCommissionOnSaleAsync(string userId, decimal saleAmount);
    Task<int> GetReferralCountAsync(string userId);
    Task UpdateProfilePictureUrlAsync(string userId, string? imageUrl);
    Task UpdateCitizenshipImageUrlAsync(string userId, string? imageUrl);
    Task<List<User>> GetMyDownlineAsync(string userId);
    Task ChangeParentAsync(string userId, string newParentId);
    Task<WalletStatementDto> GetWalletStatementAsync(string userId);
    Task<List<User>> GetDownlineAsync(string userId);

}
using backend.Dto;
using backend.Models;
namespace backend.Interface.Repository;

public interface IDistributorRepository
{
        Task<bool> CanBecomeDistributorAsync(string userId);
        Task<User?> LoginDistributorAsync(string email, string password);
        Task ChangeParentAsync(string childId, string newParentId);
        Task<bool> SignUpDistributorAsync(User user);
        Task<User?> GetDistributorByIdAsync(string id);
        Task<List<User>> GetAllDistributorsAsync();
        Task<bool> UpdateDistributorAsync(User user);
        Task<bool> DeleteDistributorAsync(string id);
        Task Addcommitsion(decimal commission, string userId);
        Task<bool> CanPlaceUnder(string parentId);
        Task<bool> IsDescendant(string rootId, string nodeId);
        Task<bool> ValidatePlacement(string referralId, string parentId);
        Task<DistributorTreeDto?> GetUserTreeAsync(string userId);
        Task<NodePosition?> GetAvailablePosition(string parentId);
        Task<List<User>> GetPeopleIReferredAsync(string myUserId);
        Task<List<User>> GetMyUplineAsync(string myUserId);
        Task ProcessCommissionOnSaleAsync(string userId, decimal saleAmount);
        Task DistributeRepurchaseCommissionAsync(string purchaserUserId, decimal repurchaseBase);
        Task<List<User>> GetMyDownlineAsync(string userId);
        Task<int> GetReferralCountAsync(string userId);
        Task UpdateRanksFromBottomAsync();
        Task<WalletStatementDto> GetWalletStatementAsync(string userId);
        Task<List<User>> GetDownlineAsync(string userId);
        Task UpdateProfilePictureUrlAsync(string userId, string? imageUrl);
        Task UpdateCitizenshipImageUrlAsync(string userId, string? imageUrl);
        Task UpdateRanksUpChainAsync(string buyerUserId, DateTime? from = null, DateTime? to = null);
        Task DistributeRepurchasePoints_FundsAndCompanyAsync(decimal repurchasePvBase, string? contextNote = null);
        Task ProcessRepurchaseAsync(string purchaserUserId, decimal repurchasePvBase);




}
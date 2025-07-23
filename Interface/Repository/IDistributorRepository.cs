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
    Task Addcommitsion(double commision, string userId);
    Task<DistributorTreeDto?> GetUserTreeAsync(string userId);
    Task<List<User>> GetPeopleIReferredAsync(string myUserId);
    Task<List<User>> GetMyUplineAsync(string myUserId);
    }
using backend.Models;

namespace backend.Interface.Repository
{
    public interface IAdminRepository
    {
        Task<IEnumerable<User>> GetAllAdminsAsync();
        Task<IEnumerable<User>> GetAdminsByRoleAsync(string role);
        Task<User?> GetAdminByIdAsync(string id);

        Task<bool> CreateAdminAsync(User user, string password);                 // legacy default
        Task<bool> CreateAdminAsync(User user, string password, string role);    // new

        Task<bool> AssignAdminRoleAsync(string userId, string role);
        Task<bool> RemoveAdminRoleAsync(string userId, string role);
        Task<bool> SetAdminRolesAsync(string userId, IEnumerable<string> roles);
        Task<IList<string>> GetUserRolesAsync(string userId);

        Task<bool> UpdateAdminAsync(User user);
        Task<bool> DeleteAdminAsync(string id);
        Task<User?> LoginAdminAsync(string email, string password);
    }

}

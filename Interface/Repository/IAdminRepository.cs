using backend.Models;

namespace backend.Interface.Repository
{
    public interface IAdminRepository
    {
        Task<IEnumerable<User>> GetAllAdminsAsync();
        Task<User?> GetAdminByIdAsync(string id);
        Task<bool> CreateAdminAsync(User user, string password);
        Task<bool> UpdateAdminAsync(User user);
        Task<bool> DeleteAdminAsync(string id);
        Task<User?> LoginAdminAsync(string email, string password);
    }
}

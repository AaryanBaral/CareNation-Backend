using backend.Dto;
using backend.Models;

namespace backend.Interface.Service
{
    public interface IAdminService
    {
        Task<IEnumerable<UserReadDto>> GetAllAdminsAsync();
        Task<UserReadDto?> GetAdminByIdAsync(string id);
        Task<bool> CreateAdminAsync(User user, string password);
        Task<bool> UpdateAdminAsync(User user);
        Task<bool> DeleteAdminAsync(string id);
        Task<string> LoginAsync(string email, string password); // returns JWT token
    }
}

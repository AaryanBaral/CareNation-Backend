using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Dto;
using backend.Models;

namespace backend.Interface.Service
{
    public interface IAdminService
    {
        // ===== Queries =====
        Task<IEnumerable<AdminReadDto>> GetAllAdminsAsync();
        Task<IEnumerable<AdminReadDto>> GetAdminsByRoleAsync(string role);
        Task<AdminReadDto?> GetAdminByIdAsync(string id);
        Task<IList<string>> GetUserRolesAsync(string userId);

        // ===== Commands (Create / Roles) =====
        Task<bool> CreateAdminAsync(User user, string role);                 // legacy create -> default Admin role
        Task<bool> CreateAdminAsync(AdminCreateDto dto, string role);            // create from DTO with specific role
        Task<bool> CreateAdminAsync(User user, string password, string role);    // create with role (pre-built user)

        Task<bool> AssignAdminRoleAsync(string userId, string role);
        Task<bool> RemoveAdminRoleAsync(string userId, string role);
        Task<bool> SetAdminRolesAsync(string userId, IEnumerable<string> roles);

        // ===== Profile ops =====
        Task<bool> UpdateAdminAsync(User user);
        Task<bool> DeleteAdminAsync(string id);

        // ===== Auth =====
        Task<string> LoginAsync(string email, string password);
    }
}

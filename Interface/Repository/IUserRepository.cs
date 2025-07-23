

using backend.Dto;
using backend.Models;

namespace backend.Interface.Repository
{
    public interface IUserRepository
    {
        Task AddUser(User user, string role, string password);
        Task UpdateUser(User user, string role);
        Task DeleteUser(string id);
        Task<User?> GetUserById(string id);
        Task<List<User>> GetAllUsersAsync();
        Task SignUp(User user, string password, string role);
        Task<User?> Login(UserLoginDto userLoginDto);
        Task<User?> GetUserByEmail(string email);
        Task<bool> CanBecomeDistributorAsync(string userId);
        Task<bool> SignUpDistributorAsync(User dto);
        Task<List<string>> LoginAndGetRole(string email, string password);
    }
}
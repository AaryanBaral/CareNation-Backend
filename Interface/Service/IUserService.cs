
using backend.Dto;

namespace backend.Interface.Service
{
    public interface IUserService
    {
        Task<UserReadDto> SignUpAsync(UserSignUpDto dto);
        Task<string> Login(UserLoginDto dto);
        Task<UserReadDto?> GetById(string id);
        Task<List<UserReadDto>> GetAll();
        Task UpdateUserAsync(UserReadDto dto, string? newPassword = null);
        Task DeleteUser(string id);
        Task<List<string>> LoginAndGetRole(string email, string password);
    }
}
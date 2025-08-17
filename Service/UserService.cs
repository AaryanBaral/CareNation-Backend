using System.Data;
using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Mapper;
using backend.Models;
using backend.Service.Jwt;
using Microsoft.AspNetCore.Authentication;


namespace backend.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _jwtService;
        public UserService(IUserRepository userRepo, ITokenService jwtService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
        }

        public async Task<UserReadDto> SignUpAsync(UserSignUpDto dto)
        {
            var existingUser = await _userRepo.GetUserByEmail(dto.Email);
            if (existingUser is not null) throw new DuplicateNameException("Email Already Exists");
            var user = dto.ToUser();
            user.Id = Guid.NewGuid().ToString();
            await _userRepo.SignUp(user, dto.Password, dto.Role);
            return user.ToReadDto(dto.Role);
        }

        public async Task<string> Login(UserLoginDto dto)
        {
            var user = await _userRepo.Login(dto) ?? throw new AuthenticationFailureException("Invalid Credentials");
            var token = await _jwtService.CreateAccessToken(user);
            return token.AccessToken;
        }

        public async Task<UserReadDto?> GetById(string id)
        {
            var user = await _userRepo.GetUserById(id);
            if (user == null) return null;

            return user.ToReadDto(GetUserRole(user));
        }

        public async Task<List<UserReadDto>> GetAll()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return [.. users.Select(u => u.ToReadDto(GetUserRole(u)))];
        }

        public async Task UpdateUserAsync(UserReadDto dto, string? newPassword = null)
        {
            var user = await _userRepo.GetUserById(dto.Id) ?? throw new Exception("User not found");
            user.UpdateUser(dto);

            await _userRepo.UpdateUser(user, dto.Role);
        }

        public async Task DeleteUser(string id)
        {
            await _userRepo.DeleteUser(id);
        }

        private static string GetUserRole(User user)
        {
            // Implement your role fetching logic here.
            // You might want to store roles in a cache or query database.
            // For now, returning a default role or fetch from repository as needed.

            return "user"; // default role placeholder
        }
        public async Task<List<string>> LoginAndGetRole(string email, string password)
        {
            var roles = await _userRepo.LoginAndGetRole(email, password);
            return roles;
        }
    }
}

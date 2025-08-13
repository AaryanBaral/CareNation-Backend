using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Models;
using Microsoft.AspNetCore.Authentication;
using backend.Service.Jwt;
using backend.Dto;
using backend.Mapper;
using Microsoft.AspNetCore.Identity;

namespace backend.Service
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepo;
        private readonly ITokenService _jwtService;
        private readonly UserManager<User> _userManager;

        public AdminService(IAdminRepository adminRepo, ITokenService jwtService, UserManager<User> userManager)
        {
            _adminRepo = adminRepo;
            _jwtService = jwtService;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserReadDto>> GetAllAdminsAsync() {
            var admins = await _adminRepo.GetAllAdminsAsync();
            var adminDtos = new List<UserReadDto>();
            foreach (var admin in admins)
            {
                var roles = await _userManager.GetRolesAsync(admin);
                var role = roles.FirstOrDefault() ?? "Admin";
                adminDtos.Add(UserMapper.ToReadDto(admin, role));
            }

            return adminDtos;
             }

        public async Task<UserReadDto?> GetAdminByIdAsync(string id)
        {
            var admin = await _adminRepo.GetAdminByIdAsync(id)?? throw new KeyNotFoundException("admin of given id not found");
            var roles = await _userManager.GetRolesAsync(admin);
            return admin.ToReadDto(roles.FirstOrDefault()?? "Admin");

    }

        public Task<bool> CreateAdminAsync(User user, string password) => _adminRepo.CreateAdminAsync(user, password);

        public Task<bool> UpdateAdminAsync(User user) => _adminRepo.UpdateAdminAsync(user);

        public Task<bool> DeleteAdminAsync(string id) => _adminRepo.DeleteAdminAsync(id);

        public async Task<string> LoginAsync(string email, string password)
        {
            var admin = await _adminRepo.LoginAdminAsync(email, password);
            if (admin == null)
                throw new AuthenticationFailureException("Invalid admin credentials.");

            var token = _jwtService.CreateAccessToken(admin);
            return token.AccessToken;
        }
    }
}

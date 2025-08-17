using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Mapper;
using backend.Models;
using backend.Repository;
using backend.Service.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepo;
    private readonly ITokenService _jwtService;
    private readonly UserManager<User> _userManager;

    public AdminService(IAdminRepository adminRepo, ITokenService jwtService, UserManager<User> userManager)
    {
        _adminRepo   = adminRepo;
        _jwtService  = jwtService;
        _userManager = userManager;
    }

    // ===== Queries =====

    public async Task<IEnumerable<AdminReadDto>> GetAllAdminsAsync()
    {
        var admins = await _adminRepo.GetAllAdminsAsync();
        var list = new List<AdminReadDto>();

        foreach (var admin in admins)
        {
            var roles = await _userManager.GetRolesAsync(admin);
            var primary = roles.FirstOrDefault() ?? AdminRoles.Admin;
            list.Add(admin.ToReadAdminDto(primary));
        }

        return list;
    }

    public async Task<IEnumerable<AdminReadDto>> GetAdminsByRoleAsync(string role)
    {
        var admins = await _adminRepo.GetAdminsByRoleAsync(role);
        var list = new List<AdminReadDto>();

        foreach (var admin in admins)
        {
            var roles = await _userManager.GetRolesAsync(admin);
            var primary = roles.FirstOrDefault() ?? AdminRoles.Admin;
            list.Add(admin.ToReadAdminDto(primary));
        }

        return list;
    }

    public async Task<AdminReadDto?> GetAdminByIdAsync(string id)
    {
        var admin = await _adminRepo.GetAdminByIdAsync(id)
                    ?? throw new KeyNotFoundException("Admin of given id not found");
        var roles = await _userManager.GetRolesAsync(admin);
        return admin.ToReadAdminDto(roles.FirstOrDefault() ?? AdminRoles.Admin);
    }

    public Task<IList<string>> GetUserRolesAsync(string userId) =>
        _adminRepo.GetUserRolesAsync(userId);

    // ===== Commands (Create / Roles) =====

    public Task<bool> CreateAdminAsync(User user, string password) =>
        _adminRepo.CreateAdminAsync(user, password);

    public async Task<bool> CreateAdminAsync(AdminCreateDto dto, string role)
    {
        var user = new User
        {
            FirstName  = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName   = dto.LastName,
            Email      = dto.Email,
            UserName   = dto.Email
        };

        return await _adminRepo.CreateAdminAsync(user, dto.Password, role);
    }

    public Task<bool> CreateAdminAsync(User user, string password, string role) =>
        _adminRepo.CreateAdminAsync(user, password, role);

    public Task<bool> AssignAdminRoleAsync(string userId, string role) =>
        _adminRepo.AssignAdminRoleAsync(userId, role);

    public Task<bool> RemoveAdminRoleAsync(string userId, string role) =>
        _adminRepo.RemoveAdminRoleAsync(userId, role);

    public Task<bool> SetAdminRolesAsync(string userId, IEnumerable<string> roles) =>
        _adminRepo.SetAdminRolesAsync(userId, roles);

    public Task<bool> UpdateAdminAsync(User user) =>
        _adminRepo.UpdateAdminAsync(user);

    public Task<bool> DeleteAdminAsync(string id) =>
        _adminRepo.DeleteAdminAsync(id);

    // ===== Auth =====

    public async Task<string> LoginAsync(string email, string password)
    {
        var admin = await _adminRepo.LoginAdminAsync(email, password);
        if (admin == null)
            throw new AuthenticationFailureException("Invalid admin credentials.");

        var token = await _jwtService.CreateAccessToken(admin);
        return token.AccessToken;
    }
}

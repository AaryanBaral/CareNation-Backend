using backend.Dto;
using backend.Interface.Service;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/admins")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            try
            {
                var token = await _adminService.LoginAsync(dto.Email, dto.Password);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdminsAsync();
            return Ok(admins);
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminById(string id)
        {
            var admin = await _adminService.GetAdminByIdAsync(id);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        // Signup (create admin)
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] AdminCreateDto dto)
        {
            // Map DTO -> User
            var user = new User
            {
                UserName    = string.IsNullOrWhiteSpace(dto.Email) ? dto.Email : dto.Email.Split('@')[0],
                Email       = dto.Email,
                FirstName   = dto.FirstName,
                MiddleName  = dto.MiddleName,
                LastName    = dto.LastName
            };

            await _adminService.CreateAdminAsync(user, dto.Password);
            return Ok("Admin created successfully.");
        }

        // Update admin (no password here)
        [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(string id, [FromBody] AdminCreateDto dto)
        {
            var user = new User
            {
                Id          = id,
                UserName    = string.IsNullOrWhiteSpace(dto.Email) ? dto.Email : dto.Email.Split('@')[0],
                Email       = dto.Email,

                FirstName   = dto.FirstName,
                MiddleName  = dto.MiddleName,
                LastName    = dto.LastName
            };

            var result = await _adminService.UpdateAdminAsync(user);
            if (!result) return NotFound("Admin not found or update failed.");
            return Ok("Admin updated successfully.");
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            var result = await _adminService.DeleteAdminAsync(id);
            if (!result) return NotFound("Admin not found or delete failed.");
            return Ok("Admin deleted successfully.");
        }
    }
}

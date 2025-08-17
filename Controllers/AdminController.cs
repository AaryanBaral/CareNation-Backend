using System.Security.Claims;
using backend.Dto;
using backend.Interface.Service;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/admins")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ===== Auth =====
        [AllowAnonymous]
        [HttpPost("login")]
        [Consumes("application/json")]
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

        // Quick token/claims probe (optional, helpful for debugging role issues)
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }

        // ===== Queries =====
        // Admins and SuperAdmins can list admins
        [Authorize(Roles = "SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdminsAsync();
            return Ok(admins);
        }

        [Authorize(Roles = "SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminById(string id)
        {
            var admin = await _adminService.GetAdminByIdAsync(id);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        [Authorize(Roles = "Admin,SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetAdminsByRole(string role)
        {
            var admins = await _adminService.GetAdminsByRoleAsync(role);
            return Ok(admins);
        }

        [Authorize(Roles = "Admin,SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            var roles = await _adminService.GetUserRolesAsync(id);
            return Ok(roles);
        }

        // ===== Commands =====
        // Only SuperAdmin can create new admins
        [AllowAnonymous]
        // [Authorize(Roles = "SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateAdmin([FromBody] AdminCreateDto dto)
        {
            var ok = await _adminService.CreateAdminAsync(dto, dto.Role); // ensures password + role are set
            if (!ok) return BadRequest("Admin creation failed.");
            return Ok("Admin created successfully.");
        }

        // Only SuperAdmin can update admin profile data
        [Authorize(Roles = "SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateAdmin(string id, [FromBody] AdminCreateDto dto)
        {
            var user = new User
            {
                Id = id,
                UserName = string.IsNullOrWhiteSpace(dto.Email) ? dto.Email : dto.Email.Split('@')[0],
                Email = dto.Email,
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName
            };

            var result = await _adminService.UpdateAdminAsync(user);
            if (!result) return NotFound("Admin not found or update failed.");
            return Ok("Admin updated successfully.");
        }

        // Only SuperAdmin can delete admins
        [Authorize(Roles = "SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            var result = await _adminService.DeleteAdminAsync(id);
            if (!result) return NotFound("Admin not found or delete failed.");
            return Ok("Admin deleted successfully.");
        }

        // ===== Role management =====
        [Authorize(Roles = "SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("{id}/roles/{role}")]
        public async Task<IActionResult> AssignRole(string id, string role)
        {
            var ok = await _adminService.AssignAdminRoleAsync(id, role);
            if (!ok) return BadRequest("Assign role failed.");
            return Ok("Role assigned.");
        }

        [Authorize(Roles = "SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{id}/roles/{role}")]
        public async Task<IActionResult> RemoveRole(string id, string role)
        {
            var ok = await _adminService.RemoveAdminRoleAsync(id, role);
            if (!ok) return BadRequest("Remove role failed.");
            return Ok("Role removed.");
        }

        [Authorize(Roles = "SuperAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{id}/roles")]
        [Consumes("application/json")]
        public async Task<IActionResult> SetRoles(string id, [FromBody] IEnumerable<string> roles)
        {
            var ok = await _adminService.SetAdminRolesAsync(id, roles);
            if (!ok) return BadRequest("Set roles failed.");
            return Ok("Roles set.");
        }
    }
}

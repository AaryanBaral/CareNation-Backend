using System.Security.Claims;
using backend.Dto;
using backend.Interface.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<SuccessResponseDto<UserReadDto>>> SignUp([FromBody] UserSignUpDto dto)
        {
            var result = await _userService.SignUpAsync(dto);
            return Ok(new SuccessResponseDto<UserReadDto> { Data = result });
        }

        [HttpPost("login")]
        public async Task<ActionResult<SuccessResponseDto<string>>> Login([FromBody] UserLoginDto dto)
        {
            var token = await _userService.Login(dto);
            return Ok(new SuccessResponseDto<string> { Data = token });
        }


        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("my-profile")]
        public async Task<ActionResult<SuccessResponseDto<UserReadDto>>> GetById()
        {
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedAccessException("Login required");
            var user = await _userService.GetById(userId);
            if (user == null) return NotFound();
            return Ok(new SuccessResponseDto<UserReadDto> { Data = user });
        }

        [HttpGet]
        public async Task<ActionResult<SuccessResponseDto<List<UserReadDto>>>> GetAll()
        {
            var users = await _userService.GetAll();
            return Ok(new SuccessResponseDto<List<UserReadDto>> { Data = users });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserReadDto dto, [FromQuery] string? newPassword = null)
        {
            if (dto.Id != id) return BadRequest("ID mismatch.");
            await _userService.UpdateUserAsync(dto, newPassword);
            return Ok(new SuccessResponseDto<string> { Data = "User updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _userService.DeleteUser(id);
            return Ok(new SuccessResponseDto<string> { Data = "User deleted successfully" });
        }

        [HttpPost("role-login")]
        public async Task<ActionResult<SuccessResponseDto<string>>> RoleLogin([FromBody] UserLoginDto dto)
        {
            var token = await _userService.LoginAndGetRole(dto.Email, dto.Password);
            return Ok(new SuccessResponseDto<List<string>> { Data = token });
        }


    }
}

using backend.Dto;
using backend.Interface.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class DistributorController : ControllerBase
    {
        private readonly IDistributorService _distributorService;

        public DistributorController(IDistributorService distributorService)
        {
            _distributorService = distributorService;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUpDistributor([FromBody] DistributorSignUpDto dto)
        {
            var userId = User.FindFirst("Id")?.Value
                ?? throw new UnauthorizedAccessException("Login required");
            try
            {
                var result = await _distributorService.SignUpDistributorAsync(userId, dto);
                if (!result)
                    return BadRequest("Failed to sign up as distributor.");

                return Ok(new { Message = "Distributor signup successful" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginDistributor([FromBody] DistributorLoginDto dto)
        {
            var distributor = await _distributorService.LoginDistributorAsync(dto);
            return Ok(distributor);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("individual")]
        public async Task<IActionResult> GetDistributorById()
        {
            var userId = User.FindFirst("Id")?.Value
                ?? throw new UnauthorizedAccessException("Login required");
            var distributor = await _distributorService.GetDistributorByIdAsync(userId);
            if (distributor == null)
                return NotFound();

            return Ok(distributor);
        }
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet]
        public async Task<IActionResult> GetAllDistributors()
        {
            var distributors = await _distributorService.GetAllDistributorsAsync();
            return Ok(distributors);
        }
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateDistributor([FromBody] DistributorSignUpDto dto)
        {
            var userId = User.FindFirst("Id")?.Value
                     ?? throw new UnauthorizedAccessException("Login required");
            var updated = await _distributorService.UpdateDistributorAsync(userId, dto);
            if (!updated)
                return NotFound();

            return Ok(new { Message = "Distributor updated successfully" });
        }
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDistributor(string id)
        {
            var deleted = await _distributorService.DeleteDistributorAsync(id);
            if (!deleted)
                return NotFound();

            return Ok(new { Message = "Distributor deleted successfully" });
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("tree")]
        public async Task<ActionResult<DistributorTreeDto?>> GetTree()
        {
            var userId = User.FindFirst("Id")?.Value
                     ?? throw new UnauthorizedAccessException("Login required");
            var tree = await _distributorService.GetUserTreeAsync(userId);
            if (tree == null)
                return NotFound();

            return Ok(tree);
        }
    }
}

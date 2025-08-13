using System.Security.Claims;
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
        private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("Please login.");

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUpDistributor([FromForm] DistributorSignUpDto dto, [FromForm] IFormFile citizenshipFile, [FromForm] IFormFile? profilePicture = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Login required");
            try
            {
                var result = await _distributorService.SignUpDistributorAsync(userId, dto, citizenshipFile, profilePicture);
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

        [Authorize(Policy = "SensitiveAction")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("change-parent")]
        public async Task<IActionResult> ChangeParent([FromBody] ChangeParentRequest changeParentRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Login required");

            if (string.IsNullOrEmpty(changeParentRequest.NewParentId))
                return BadRequest("New parent ID is required");

            if (string.IsNullOrEmpty(changeParentRequest.ChildId))
                return BadRequest("Child ID is required");
            await _distributorService.ChangeParentAsync(userId, changeParentRequest.NewParentId, changeParentRequest.ChildId);

            // Optionally, you can return the updated distributor info or a success message
            // var updatedDistributor = await _distributorService.GetDistributorByIdAsync(childId);
            // return Ok(updatedDistributor);
            return Ok("Update successful");
        }
        [Authorize(Policy = "SensitiveAction")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("total-referrals")]
        public async Task<IActionResult> GetTotalReferrals()
        {
            var userId = GetUserId();
            var total = await _distributorService.GetTotalReferralsAsync(userId);
            return Ok(new { totalReferrals = total });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginDistributor([FromBody] DistributorLoginDto dto)
        {
            var distributor = await _distributorService.LoginDistributorAsync(dto);
            return Ok(distributor);
        }

        [Authorize(Policy = "SensitiveAction")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("individual")]
        public async Task<IActionResult> GetDistributorById()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"Fetching distributor for userId: {userId}");
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
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
        [HttpGet("me/downline")]
        public async Task<ActionResult<List<DownlineUserDto>>> GetMyDownline([FromQuery] string? search = null)
        {
            var meId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(meId)) return Unauthorized();

            var list = await _distributorService.GetDownlineAsync(meId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                list = list
                    .Where(u =>
                        (u.FullName ?? "").ToLower().Contains(s) ||
                        (u.Email ?? "").ToLower().Contains(s) ||
                        (u.PhoneNumber ?? "").ToLower().Contains(s))
                    .ToList();
            }

            return Ok(list);
        }

        [Authorize(Policy = "SensitiveAction")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("tree")]
        public async Task<ActionResult<DistributorTreeDto?>> GetTree()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException("Login required");
            var tree = await _distributorService.GetUserTreeAsync(userId);
            if (tree == null)
                return NotFound();

            return Ok(tree);
        }
        [Authorize(Policy = "SensitiveAction")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("total-downlines")]
        public async Task<IActionResult> GetTotalDownlines()
        {
            var userId = GetUserId();
            var total = await _distributorService.GetTotalDownlineAsync(userId);
            return Ok(new { totalDownlines = total });
        }

        [Authorize(Policy = "SensitiveAction")]
        [HttpGet("statement/{userId}")]
        public async Task<ActionResult<WalletStatementDto>> GetStatement(string userId)
        {
            var statement = await _distributorService.GetWalletStatementAsync(userId);
            return Ok(statement);
        }


        [Authorize(Policy = "SensitiveAction")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("statement/me")]
        public async Task<ActionResult<WalletStatementDto>> GetMyStatement()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // or however you get the current userId
            if (userId == null) return Unauthorized();
            var statement = await _distributorService.GetWalletStatementAsync(userId);
            return Ok(statement);
        }
    }
}

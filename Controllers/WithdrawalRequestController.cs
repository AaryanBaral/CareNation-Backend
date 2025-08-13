
using System.Security.Claims;
using backend.Interface.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize(Policy = "SensitiveAction")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/withdrawals")]
    public class WithdrawalRequestController : ControllerBase
    {
        private readonly IWithdrawalRequestService _service;

        public WithdrawalRequestController(IWithdrawalRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<WithdrawalRequestDto>>> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<WithdrawalRequestDto>>> Search(string keyword)
        {
            var data = await _service.SearchAsync(keyword);
            return Ok(data);
        }

        [HttpPost("request")]
        public async Task<ActionResult<WithdrawalRequestDto>> RequestWithdrawal([FromBody] WithdrawalRequestCreateDto dto)
        {
        string userId = (User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new UnauthorizedAccessException("Please login to view this");
            var data = await _service.CreateAsync(userId!, dto.Amount, dto.Remarks);
            return Ok(data);
        }

        [HttpPut("{id}/approve")]
        public async Task<ActionResult> Approve(int id, [FromBody] string remarks = "")
        {
            var result = await _service.ApproveAsync(id, remarks);
            return result ? Ok("Approved") : BadRequest("Unable to approve.");
        }

        [HttpPut("{id}/reject")]
        public async Task<ActionResult> Reject(int id, [FromBody] string remarks = "")
        {
            var result = await _service.RejectAsync(id, remarks);
            return result ? Ok("Rejected") : BadRequest("Unable to reject.");
        }
    }

    public class WithdrawalRequestCreateDto
    {
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
    }
}

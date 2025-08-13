using backend.Interface.Service;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize(Policy = "SensitiveAction")]
    [ApiController]
    [Route("api/[controller]")]
    public class CommissionPayoutController : ControllerBase
    {
        private readonly ICommissionPayoutService _service;

        public CommissionPayoutController(ICommissionPayoutService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payouts = await _service.GetAllAsync();
            return Ok(payouts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payout = await _service.GetByIdAsync(id);
            return payout == null ? NotFound() : Ok(payout);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var payouts = await _service.GetByUserIdAsync(userId);
            return Ok(payouts);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommissionPayout payout)
        {
            await _service.AddAsync(payout);
            return Ok(new { message = "Payout created" });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status, [FromQuery] string? remarks = null)
        {
            var result = await _service.UpdateStatusAsync(id, status, remarks);
            return result ? Ok(new { message = "Status updated" }) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? Ok(new { message = "Payout deleted" }) : NotFound();
        }
    }
}

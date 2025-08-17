// Controllers/AuthController.Impersonation.cs
using System.Security.Claims;
using backend.Data;
using backend.Dto.Auth;
using backend.Models;
using backend.Service.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _cfg;

        public AuthController(AppDbContext db, UserManager<User> userManager, ITokenService tokenService, IConfiguration cfg)
        {
            _db = db; _userManager = userManager; _tokenService = tokenService; _cfg = cfg;
        }

        // 6A) ADMIN: start impersonation
        [HttpPost("impersonation/start")]
        public async Task<ActionResult<StartImpersonationResponse>> StartImpersonation([FromBody] StartImpersonationDto dto)
        {
            // validate target exists
            var target = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == dto.TargetUserId);
            if (target == null) return NotFound();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)?? throw new UnauthorizedAccessException("Please login to view this");

            var ticket = new ImpersonationTicket
            {
                AdminId = adminId,
                TargetUserId = target.Id,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(60),
                Reason = dto.Reason,
                ReturnUrl = dto.ReturnUrl
            };
            _db.ImpersonationTickets.Add(ticket);
            await _db.SaveChangesAsync();

            var url = $"{_cfg["Portals:DistributorBaseUrl"]?.TrimEnd('/')}/profile"; // landing page (frontend) to POST the code

            return new StartImpersonationResponse
            {
                Code = ticket.Code,
                DistributorImpersonationUrl = url!
            };
        }

        // 6B) DISTRIBUTOR: redeem code and issue impersonation token
        [AllowAnonymous]
        [HttpPost("impersonation/redeem")]
        public async Task<ActionResult<object>> RedeemImpersonation([FromBody] RedeemImpersonationDto dto)
        {
            var ticket = await _db.ImpersonationTickets.FirstOrDefaultAsync(t => t.Code == dto.Code);
            if (ticket == null) return Unauthorized();

            if (ticket.Used || ticket.ExpiresAt < DateTimeOffset.UtcNow) return Unauthorized();

            // mark used atomically
            ticket.Used = true;
            ticket.UsedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            var target = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == ticket.TargetUserId);
            if (target == null) return Unauthorized();

            // extra impersonation claims
            var extraClaims = new[]
            {
                new System.Security.Claims.Claim("impersonation", "true"),
                new System.Security.Claims.Claim("impersonated_by", ticket.AdminId),
                new System.Security.Claims.Claim("acr", "impersonation")
            };

            var tokens = await _tokenService.CreateAccessToken(target, extraClaims, isImpersonation: true);

            return Ok(new
            {
                accessToken = tokens.AccessToken,
                expiresAtUtc = tokens.ExpiresAt,
                returnUrl = ticket.ReturnUrl
            });
        }

        // 6C) DISTRIBUTOR: re-enter password to refresh auth_time
        [Authorize] // distributor only; impersonation sessions will just not need this endpoint
        [HttpPost("reauth")]
        public async Task<ActionResult<object>> Reauth([FromBody] ReauthDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid) return Unauthorized();

            // New token pair with fresh auth_time
            var tokens = await _tokenService.CreateAccessToken(user);

            return Ok(new
            {
                accessToken = tokens.AccessToken,
                expiresAtUtc = tokens.ExpiresAt
            });
        }
    }
}

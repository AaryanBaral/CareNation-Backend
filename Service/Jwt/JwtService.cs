// backend/Service/Jwt/JwtService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using backend.Interface.Service;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace backend.Service.Jwt
{


    public class TokenService(UserManager<User> userManager, IConfiguration cfg) : ITokenService
    {
        private readonly IConfiguration _cfg = cfg;
        private UserManager<User> _userManager = userManager;

        public async Task<TokenPair> CreateAccessToken(
            User user,
            IEnumerable<Claim>? extraClaims = null,
            bool isImpersonation = false,
            IList<string>? roles = null)
        {
            // ---- Config
            var issuer = _cfg["Jwt:Issuer"];
            var audience = _cfg["Jwt:Audience"];
            var keyBytes = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing"));
            var key = new SymmetricSecurityKey(keyBytes);

            var minutes = isImpersonation
                ? int.Parse(_cfg["Jwt:ImpersonationMinutes"] ?? "45")
                : int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "6000");

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var iat = new DateTimeOffset(now).ToUnixTimeSeconds().ToString();
            roles = await _userManager.GetRolesAsync(user);

            // ---- Base claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new("Id", user.Id),
                new(ClaimTypes.Name, user.FirstName ?? user.UserName ?? user.Email ?? "User"),
                new(JwtRegisteredClaimNames.Sub, user.Id),                       // stable subject = userId
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, iat, ClaimValueTypes.Integer64)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            // ---- Roles (both standard and "role")
            if (roles != null)
            {
                foreach (var r in roles)
                {
                    if (!string.IsNullOrWhiteSpace(r))
                    {
                        claims.Add(new(ClaimTypes.Role, r));
                        claims.Add(new("role", r));
                    }
                }
            }

            // ---- Normal vs Impersonation
            if (isImpersonation)
            {
                // Ensure required impersonation claims (extraClaims may also include these)
                if (extraClaims is null || !extraClaims.Any(c => c.Type == "impersonation"))
                    claims.Add(new("impersonation", "true"));

                // acr for context
                if (extraClaims is null || !extraClaims.Any(c => c.Type == "acr"))
                    claims.Add(new("acr", "impersonation"));
            }
            else
            {
                // Normal login tokens: recent password + exempt from reauth prompts in policy
                claims.Add(new("auth_time", iat));
                claims.Add(new("amr", "pwd"));
                claims.Add(new("reauth_exempt", "true"));
            }

            // ---- Merge any extra claims last (so caller can override/add)
            if (extraClaims != null) claims.AddRange(extraClaims);

            // ---- Build JWT
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(minutes),   // normal has AccessTokenMinutes; impersonation is short-lived
                signingCredentials: creds
            );

            var access = new JwtSecurityTokenHandler().WriteToken(token);
            return new TokenPair(access, token.ValidTo);
        }
    }
}


using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Configurations;
using backend.Data;
using backend.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace backend.Service.Jwt
{
    public class JwtService(
        IOptions<JwtConfig> config) : IJwtService
    {
        private readonly JwtConfig _config = config.Value;

        public string GenerateJwtToken(User user)
        {
            try
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();

                // convert the string into byte of arrays
                var key = Encoding.UTF8.GetBytes(_config.Secret);
                /* Claims
                    this is used to add key-value pair of data that should be encrypted
                    and added to the jwt token
                */
                var claims = new ClaimsIdentity([
                    new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub,
                    user.Email ?? throw new ArgumentNullException(nameof(user), "User's Email cannot be null")),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            ]);

                /*
                    A token descriptor describes the properites and values to be in the token
                */
                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = claims,
                    Expires = DateTime.UtcNow.Add(_config.ExpiryTimeFrame),
                    SigningCredentials =
                        new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                };
                var token = jwtTokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = jwtTokenHandler.WriteToken(token);


                // //  creating a new refresh token
                // var refreshToken = new RefreshToken()
                // {
                //     JwtId = token.Id,
                //     Token = RandomStringGenerator(23), // Generate a refresh token
                //     ExpiryDate = DateTime.UtcNow.AddMonths(6),
                //     UserId = user.Id,
                //     IsRevoked = false,
                //     IsUsed = false,
                //     AddedDate = DateTime.UtcNow,
                // };

                // adding the refresh token to the database
                // await _context.RefreshTokens.AddAsync(refreshToken);
                // await _context.SaveChangesAsync();

                return jwtToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Server Error {ex.Message}");
            }
        }
    }
}
// Services/ITokenService.cs
using backend.Models;
using System.Security.Claims;

namespace backend.Interface.Service
{
    public record TokenPair(string AccessToken, DateTime ExpiresAt /* add RefreshToken if you use it */);
}

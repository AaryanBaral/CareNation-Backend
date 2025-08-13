

using System.Security.Claims;
using backend.Interface.Service;
using backend.Models;

namespace backend.Service.Jwt
{
    public interface ITokenService
    {
        TokenPair CreateAccessToken(
            User user,
            IEnumerable<Claim>? extraClaims = null,
            bool isImpersonation = false,
            IEnumerable<string>? roles = null
        );
    }

}
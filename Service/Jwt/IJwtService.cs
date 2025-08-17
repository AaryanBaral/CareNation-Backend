

using System.Security.Claims;
using backend.Interface.Service;
using backend.Models;

namespace backend.Service.Jwt
{
    public interface ITokenService
    {
        Task<TokenPair> CreateAccessToken(
                    User user,
                    IEnumerable<Claim>? extraClaims = null,
                    bool isImpersonation = false,
                    IList<string>? roles = null);
    }

}
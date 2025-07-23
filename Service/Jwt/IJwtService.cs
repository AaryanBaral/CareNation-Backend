

using backend.Models;

namespace backend.Service.Jwt
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user);
    }
}
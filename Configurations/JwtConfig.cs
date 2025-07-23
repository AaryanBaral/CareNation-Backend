

namespace backend.Configurations
{
    public class JwtConfig
    {
        public required string Secret { get; set; }
        public required TimeSpan ExpiryTimeFrame { get; set; }
    }
}
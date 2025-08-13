// Auth/RecentReauthPolicy.cs
using Microsoft.AspNetCore.Authorization;

namespace backend.Auth
{
    public class RecentReauthRequirement : IAuthorizationRequirement
    {
        public TimeSpan MaxAge { get; }
        public RecentReauthRequirement(TimeSpan maxAge) => MaxAge = maxAge;
    }

    public class RecentReauthHandler : AuthorizationHandler<RecentReauthRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RecentReauthRequirement req)
        {
            if (context.User.HasClaim("impersonation", "true"))
            {
                context.Succeed(req);
                return Task.CompletedTask;
            }

            // NEW: normal logins that are exempt from reauth
            if (context.User.HasClaim("reauth_exempt", "true"))
            {
                context.Succeed(req);
                return Task.CompletedTask;
            }

            var authTime = context.User.FindFirst("auth_time")?.Value;
            if (long.TryParse(authTime, out var unix))
            {
                var when = DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
                if (DateTime.UtcNow - when <= req.MaxAge)
                {
                    context.Succeed(req);
                    return Task.CompletedTask;
                }
            }

            context.Fail();
            return Task.CompletedTask;
        }
    }

}

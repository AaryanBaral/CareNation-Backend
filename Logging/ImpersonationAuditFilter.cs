// Logging/ImpersonationAuditFilter.cs
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace backend.Logging
{
    public class ImpersonationAuditFilter : IActionFilter
    {
        private readonly ILogger<ImpersonationAuditFilter> _logger;
        public ImpersonationAuditFilter(ILogger<ImpersonationAuditFilter> logger) => _logger = logger;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var u = context.HttpContext.User;
            if (u?.HasClaim("impersonation", "true") == true)
            {
                var actedAs = u.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var admin = u.FindFirst("impersonated_by")?.Value;
                _logger.LogInformation("IMPERSONATION: admin {AdminId} acting as {UserId} → {Path}",
                    admin, actedAs, context.HttpContext.Request.Path);
            }
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}

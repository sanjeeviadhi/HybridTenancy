using HybridTenancy.Application.Services;

namespace HybridTenancy.Service.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;
        private static readonly string[] ExcludedPaths = ["/", "/swagger", "/health", "/favicon.ico"];

        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            try
            {
                var requestPath = context.Request.Path.Value?.ToLowerInvariant();
                if (ExcludedPaths.Any(path => requestPath == path || requestPath.StartsWith(path)))
                {
                    await _next(context);
                    return;
                }

                var user = context.User;
                if (user.Identity?.IsAuthenticated != true)
                {
                    _logger.LogWarning("Unauthorized access attempt. JWT missing.");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized - JWT required.");
                    return;
                }

                var tenantIdClaim = user.Claims.FirstOrDefault(c => c.Type == "TenantId");
                if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    _logger.LogWarning("Missing or invalid TenantId in JWT claims.");
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid or missing TenantId in token.");
                    return;
                }

                var tenant = await tenantService.GetTenantAsync(tenantId);
                if (tenant == null || tenant.ValidTill < DateTime.UtcNow)
                {
                    _logger.LogWarning("Tenant not found or subscription expired. TenantId: {TenantId}", tenantId);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Tenant not found or expired.");
                    return;
                }

                context.Items["TenantInfo"] = tenant;
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in TenantResolutionMiddleware.");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An internal server error occurred during tenant resolution.");
            }
        }
    }
}

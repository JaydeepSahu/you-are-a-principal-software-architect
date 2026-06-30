using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Api.Security;

public sealed class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, TenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(tenantContext);

        TenantId? tenantId = null;
        string? tenantClaimValue = httpContext.User.FindFirst(IdentityClaimTypes.TenantId)?.Value;
        if (Guid.TryParse(tenantClaimValue, out Guid tenantGuid))
        {
            tenantId = TenantId.From(tenantGuid);
        }

        string? applicationId = httpContext.User.FindFirst(IdentityClaimTypes.ApplicationId)?.Value;
        tenantContext.Set(tenantId, applicationId, httpContext.GetCorrelationId());

        await _next(httpContext);
    }
}

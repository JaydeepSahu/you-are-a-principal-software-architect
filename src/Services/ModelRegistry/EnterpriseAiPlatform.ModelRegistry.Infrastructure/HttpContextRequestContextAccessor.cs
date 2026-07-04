using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.AspNetCore.Http;

namespace EnterpriseAiPlatform.ModelRegistry.Infrastructure;

public sealed class HttpContextRequestContextAccessor(IHttpContextAccessor httpContextAccessor) : IRequestContextAccessor
{
    public RequestContext Current
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No HTTP context available.");

            var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value;
            var tenantId = tenantClaim is not null && Guid.TryParse(tenantClaim, out var parsedTenantId)
                ? TenantId.From(parsedTenantId)
                : TenantId.From(Guid.Parse("00000000-0000-0000-0000-000000000001"));

            return new RequestContext(
                tenantId,
                httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? httpContext.TraceIdentifier,
                httpContext.User.FindFirst("sub")?.Value,
                httpContext.User.FindFirst("application_id")?.Value);
        }
    }
}

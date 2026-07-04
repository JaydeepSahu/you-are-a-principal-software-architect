using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.AspNetCore.Http;

namespace EnterpriseAiPlatform.Knowledge.Infrastructure;

public sealed class HttpContextRequestContextAccessor(IHttpContextAccessor httpContextAccessor) : IRequestContextAccessor
{
    public RequestContext Current
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No HTTP context available.");
            var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value;
            var userClaim = httpContext.User.FindFirst("sub")?.Value;
            var appClaim = httpContext.User.FindFirst("application_id")?.Value;
            var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? httpContext.TraceIdentifier;

            return new RequestContext(
                tenantClaim is not null && Guid.TryParse(tenantClaim, out var tenantId)
                    ? TenantId.From(tenantId)
                    : TenantId.From(Guid.Parse("00000000-0000-0000-0000-000000000001")),
                correlationId,
                userClaim,
                appClaim);
        }
    }
}

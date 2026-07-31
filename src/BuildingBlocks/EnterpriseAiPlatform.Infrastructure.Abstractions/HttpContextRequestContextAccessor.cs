using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace EnterpriseAiPlatform.Infrastructure.Abstractions;

/// <summary>
/// Reads the current request context from the active HTTP context.
/// Tenant isolation is mandatory: requests without a valid tenant_id claim
/// are rejected immediately — there is no silent fallback.
/// </summary>
public sealed class HttpContextRequestContextAccessor(
    IHttpContextAccessor httpContextAccessor,
    IHostEnvironment environment) : IRequestContextAccessor
{
    public RequestContext Current
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No HTTP context is available.");

            var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value;

            TenantId tenantId;
            if (tenantClaim is not null && Guid.TryParse(tenantClaim, out var parsedTenantId))
            {
                tenantId = TenantId.From(parsedTenantId);
            }
            else if (environment.IsDevelopment())
            {
                // Development-only: use a well-known system tenant so local
                // testing works without full auth flows. Never allowed in prod.
                tenantId = TenantId.From(new Guid("00000000-0000-0000-0000-000000000001"));
            }
            else
            {
                throw new UnauthorizedAccessException(
                    "Request is missing the required tenant_id claim.");
            }

            return new RequestContext(
                tenantId,
                httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                    ?? httpContext.TraceIdentifier,
                httpContext.User.FindFirst("sub")?.Value,
                httpContext.User.FindFirst("application_id")?.Value);
        }
    }
}

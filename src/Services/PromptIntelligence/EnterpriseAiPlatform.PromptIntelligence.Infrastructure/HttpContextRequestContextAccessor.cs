using System.Collections.Concurrent;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.AspNetCore.Http;

namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure;

public sealed class HttpContextRequestContextAccessor : IRequestContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextRequestContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public RequestContext Current
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                throw new InvalidOperationException("No HTTP context available.");
            }

            var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value;
            var userClaim = httpContext.User.FindFirst("sub")?.Value;
            var appClaim = httpContext.User.FindFirst("application_id")?.Value;

            var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? httpContext.TraceIdentifier;

            return new RequestContext(
                tenantClaim is not null && Guid.TryParse(tenantClaim, out var tid)
                    ? TenantId.From(tid)
                    : TenantId.From(Guid.Empty),
                correlationId,
                userClaim,
                appClaim);
        }
    }
}

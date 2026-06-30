using System.Security.Claims;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Api.Security;

public sealed class AuditRequestMiddleware
{
    private readonly RequestDelegate _next;

    public AuditRequestMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        IAuditLogger auditLogger,
        IIdentityUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        await _next(httpContext);

        if (!httpContext.Request.Path.StartsWithSegments("/api/v1/identity", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        TenantId? tenantId = null;
        string? tenantClaim = httpContext.User.FindFirst(IdentityClaimTypes.TenantId)?.Value;
        if (Guid.TryParse(tenantClaim, out Guid tenantGuid))
        {
            tenantId = TenantId.From(tenantGuid);
        }

        string outcome = httpContext.Response.StatusCode < StatusCodes.Status400BadRequest ? "succeeded" : "failed";
        await auditLogger.LogAsync(
            new AuditLogRequest(
                tenantId,
                AuditActionNames.ApiRequestCompleted,
                outcome,
                httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                httpContext.User.FindFirst(IdentityClaimTypes.ApplicationId)?.Value,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.GetCorrelationId()),
            httpContext.RequestAborted);

        await unitOfWork.SaveChangesAsync(httpContext.RequestAborted);
    }
}

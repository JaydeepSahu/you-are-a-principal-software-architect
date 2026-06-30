using System.Security.Claims;
using EnterpriseAiPlatform.AiGateway.Application;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.AiGateway.Api.Security;

public interface IGatewayPrincipalContextAccessor
{
    GatewayPrincipalContext Current { get; }
}

public sealed class GatewayPrincipalContextAccessor : IGatewayPrincipalContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GatewayPrincipalContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public GatewayPrincipalContext Current
    {
        get
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("Gateway principal context requires an active HTTP context.");

            ClaimsPrincipal user = httpContext.User;
            string? tenantClaim = user.FindFirst(IdentityClaimTypes.TenantId)?.Value;
            string subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? throw new InvalidOperationException("Authenticated principal is missing a subject identifier.");

            if (!Guid.TryParse(tenantClaim, out Guid tenantGuid))
            {
                throw new InvalidOperationException("Authenticated principal is missing a valid tenant identifier.");
            }

            return new GatewayPrincipalContext(
                TenantId.From(tenantGuid),
                subjectId,
                user.FindFirst(IdentityClaimTypes.ApplicationId)?.Value,
                user.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct(StringComparer.Ordinal).ToArray(),
                user.FindFirst(IdentityClaimTypes.AuthenticationMethod)?.Value ?? AiGatewayAuthenticationSchemes.PlatformJwt);
        }
    }
}

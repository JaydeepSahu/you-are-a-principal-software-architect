using System.Security.Claims;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.Identity.Domain;
using Microsoft.AspNetCore.Authentication;

namespace EnterpriseAiPlatform.Identity.Api.Security;

public sealed class EntraClaimsTransformation : IClaimsTransformation
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserAccountRepository _userAccountRepository;

    public EntraClaimsTransformation(
        ITenantRepository tenantRepository,
        IUserAccountRepository userAccountRepository)
    {
        _tenantRepository = tenantRepository;
        _userAccountRepository = userAccountRepository;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity?.IsAuthenticated != true
            || principal.HasClaim(claim => claim.Type == IdentityClaimTypes.TenantId))
        {
            return principal;
        }

        string? entraTenantValue = principal.FindFirstValue(IdentityClaimTypes.EntraTenantId);
        string? entraObjectValue = principal.FindFirstValue(IdentityClaimTypes.EntraObjectId);

        if (!Guid.TryParse(entraTenantValue, out Guid entraTenantId)
            || !Guid.TryParse(entraObjectValue, out Guid entraObjectId))
        {
            return principal;
        }

        Tenant? tenant = await _tenantRepository.GetByEntraTenantIdAsync(entraTenantId);
        if (tenant is null || !tenant.IsActive())
        {
            return principal;
        }

        UserAccount? userAccount = await _userAccountRepository.GetByEntraObjectIdAsync(tenant.Id, entraObjectId);
        if (userAccount is null || !userAccount.IsActive())
        {
            return principal;
        }

        ClaimsIdentity identity = new(
            IdentityAuthenticationSchemes.EntraId,
            ClaimTypes.Name,
            ClaimTypes.Role);
        identity.AddClaim(new Claim(IdentityClaimTypes.TenantId, tenant.Id.Value.ToString("D")));
        identity.AddClaim(new Claim(IdentityClaimTypes.AuthenticationMethod, IdentityAuthenticationSchemes.EntraId));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userAccount.Id.ToString("D")));
        identity.AddClaims(userAccount.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        principal.AddIdentity(identity);
        return principal;
    }
}

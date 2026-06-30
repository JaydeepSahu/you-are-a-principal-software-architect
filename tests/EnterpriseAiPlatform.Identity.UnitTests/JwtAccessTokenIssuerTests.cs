using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.Identity.Infrastructure.Security;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.Identity.UnitTests;

public sealed class JwtAccessTokenIssuerTests
{
    [Fact]
    public void IssuedTokenContainsTenantApplicationAndRoleClaims()
    {
        PlatformJwtOptions options = new()
        {
            Issuer = "https://identity.enterprise-ai-platform.test",
            Audience = "enterprise-ai-platform",
            SigningKey = "0123456789abcdef0123456789abcdef",
            AccessTokenLifetimeMinutes = 15
        };
        JwtAccessTokenIssuer issuer = new(Options.Create(options), TimeProvider.System);
        TenantId tenantId = TenantId.New();

        IssuedAccessToken issuedToken = issuer.Issue(new AccessTokenRequest(
            tenantId,
            "subject-1",
            "ide-extension",
            [IdentityRoles.IdeExtension, IdentityRoles.Developer]));

        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(issuedToken.Value);

        Assert.Equal(options.Issuer, jwt.Issuer);
        Assert.Contains(jwt.Audiences, audience => audience == options.Audience);
        Assert.Contains(jwt.Claims, claim => claim.Type == IdentityClaimTypes.TenantId && claim.Value == tenantId.Value.ToString("D"));
        Assert.Contains(jwt.Claims, claim => claim.Type == IdentityClaimTypes.ApplicationId && claim.Value == "ide-extension");
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == IdentityRoles.IdeExtension);
        Assert.True(issuedToken.ExpiresAtUtc > TimeProvider.System.GetUtcNow());
    }
}

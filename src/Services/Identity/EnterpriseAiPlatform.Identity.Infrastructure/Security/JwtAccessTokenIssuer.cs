using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Security;

public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly PlatformJwtOptions _options;
    private readonly TimeProvider _timeProvider;

    public JwtAccessTokenIssuer(IOptions<PlatformJwtOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public IssuedAccessToken Issue(AccessTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] signingKeyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        if (signingKeyBytes.Length < 32)
        {
            throw new InvalidOperationException("Platform JWT signing key must be at least 256 bits.");
        }

        DateTimeOffset nowUtc = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAtUtc = nowUtc.AddMinutes(_options.AccessTokenLifetimeMinutes);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, request.SubjectId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("D")),
            new(IdentityClaimTypes.TenantId, request.TenantId.Value.ToString("D")),
            new(IdentityClaimTypes.AuthenticationMethod, "PlatformJwt")
        ];

        if (!string.IsNullOrWhiteSpace(request.ApplicationId))
        {
            claims.Add(new Claim(IdentityClaimTypes.ApplicationId, request.ApplicationId));
        }

        claims.AddRange(request.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        SigningCredentials signingCredentials = new(
            new SymmetricSecurityKey(signingKeyBytes),
            SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            _options.Issuer,
            _options.Audience,
            claims,
            nowUtc.UtcDateTime,
            expiresAtUtc.UtcDateTime,
            signingCredentials);

        return new IssuedAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}

using System.Security.Claims;
using System.Text.Encodings.Web;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.Identity.Api.Security;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyAuthenticator _apiKeyAuthenticator;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyAuthenticator apiKeyAuthenticator)
        : base(options, logger, encoder)
    {
        _apiKeyAuthenticator = apiKeyAuthenticator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out Microsoft.Extensions.Primitives.StringValues apiKeyValues))
        {
            return AuthenticateResult.NoResult();
        }

        string? apiKey = apiKeyValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return AuthenticateResult.Fail("API key header is empty.");
        }

        EnterpriseAiPlatform.SharedKernel.Result<AuthenticatedApiKeyPrincipal> result =
            await _apiKeyAuthenticator.AuthenticateAsync(
                new ApiKeyAuthenticationContext(
                    apiKey,
                    Context.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString(),
                    Context.GetCorrelationId()),
                Context.RequestAborted);

        if (result.IsFailure)
        {
            return AuthenticateResult.Fail(result.Error.Message);
        }

        AuthenticatedApiKeyPrincipal principal = result.Value;
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, principal.ApiKeyId.ToString("D")),
            new(IdentityClaimTypes.TenantId, principal.TenantId.Value.ToString("D")),
            new(IdentityClaimTypes.ApplicationId, principal.ApplicationId),
            new(IdentityClaimTypes.AuthenticationMethod, IdentityAuthenticationSchemes.ApiKey)
        ];
        claims.AddRange(principal.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        ClaimsIdentity identity = new(claims, Scheme.Name, ClaimTypes.NameIdentifier, ClaimTypes.Role);
        ClaimsPrincipal claimsPrincipal = new(identity);
        AuthenticationTicket ticket = new(claimsPrincipal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}

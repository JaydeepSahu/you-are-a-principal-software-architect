using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public sealed record ApiKeyAuthenticationContext(
    string ApiKey,
    string? IpAddress,
    string? UserAgent,
    string CorrelationId);

public sealed record AuthenticatedApiKeyPrincipal(
    TenantId TenantId,
    Guid ApiKeyId,
    string ApplicationId,
    IReadOnlyCollection<string> Roles);

public interface IApiKeyAuthenticator
{
    Task<Result<AuthenticatedApiKeyPrincipal>> AuthenticateAsync(
        ApiKeyAuthenticationContext context,
        CancellationToken cancellationToken = default);
}

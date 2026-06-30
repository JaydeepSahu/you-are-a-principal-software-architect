using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.Tokens;

public sealed record ExchangeApiKeyForTokenCommand(
    string ApiKey,
    string? IpAddress,
    string? UserAgent,
    string CorrelationId) : ICommand<ApiKeyTokenResult>;

public sealed record ApiKeyTokenResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    TenantId TenantId,
    string SubjectId,
    IReadOnlyCollection<string> Roles);

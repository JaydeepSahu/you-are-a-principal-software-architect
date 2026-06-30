namespace EnterpriseAiPlatform.Identity.Contracts;

public sealed record CreateApiKeyRequest(
    Guid TenantId,
    string ApplicationId,
    string Name,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset ExpiresAtUtc);

public sealed record CreateApiKeyResponse(
    Guid ApiKeyId,
    Guid TenantId,
    string Name,
    string KeyPrefix,
    string ApiKey,
    DateTimeOffset ExpiresAtUtc);

public sealed record ApiKeyTokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    Guid TenantId,
    string SubjectId,
    IReadOnlyCollection<string> Roles);

public sealed record RefreshTokenRequest(string RefreshToken);

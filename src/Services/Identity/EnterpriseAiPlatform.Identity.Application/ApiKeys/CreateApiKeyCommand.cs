using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.ApiKeys;

public sealed record CreateApiKeyCommand(
    TenantId TenantId,
    string ApplicationId,
    string Name,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset ExpiresAtUtc,
    string? RequestedBySubjectId,
    string? IpAddress,
    string? UserAgent,
    string CorrelationId) : ICommand<CreateApiKeyResult>;

public sealed record CreateApiKeyResult(
    Guid ApiKeyId,
    TenantId TenantId,
    string Name,
    string KeyPrefix,
    string ApiKey,
    DateTimeOffset ExpiresAtUtc);

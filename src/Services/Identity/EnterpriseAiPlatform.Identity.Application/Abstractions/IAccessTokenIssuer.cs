using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public sealed record AccessTokenRequest(
    TenantId TenantId,
    string SubjectId,
    string? ApplicationId,
    IReadOnlyCollection<string> Roles);

public sealed record IssuedAccessToken(string Value, DateTimeOffset ExpiresAtUtc);

public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(AccessTokenRequest request);
}

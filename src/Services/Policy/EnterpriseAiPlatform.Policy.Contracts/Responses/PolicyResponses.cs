using EnterpriseAiPlatform.Policy.Domain;

namespace EnterpriseAiPlatform.Policy.Contracts.Responses;

public sealed record PolicyCreatedResponse(
    Guid PolicyId,
    DateTimeOffset CreatedAtUtc);

public sealed record PolicyEvaluatedResponse(
    bool Allowed,
    Guid? MatchedPolicyId,
    string Effect,
    string? Reason);

public sealed record PolicyListResponse(
    int TotalCount,
    IReadOnlyList<PolicySummaryResponse> Policies);

public sealed record PolicySummaryResponse(
    Guid Id,
    string Name,
    PolicyResourceType ResourceType,
    PolicyEffect Effect,
    bool IsEnabled,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAtUtc);

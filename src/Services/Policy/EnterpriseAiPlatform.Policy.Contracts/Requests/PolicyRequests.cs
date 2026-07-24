using EnterpriseAiPlatform.Policy.Domain;

namespace EnterpriseAiPlatform.Policy.Contracts.Requests;

public sealed record CreatePolicyRequest(
    string Name,
    string Description,
    PolicyResourceType ResourceType,
    PolicyEffect Effect,
    IReadOnlyList<string> Principals,
    IReadOnlyList<string> Actions,
    IReadOnlyList<string> Conditions,
    DateTimeOffset? ExpiresAtUtc);

public sealed record EvaluatePolicyRequest(
    PolicyResourceType ResourceType,
    string Action,
    string? Principal = null,
    IReadOnlyDictionary<string, string>? Context = null);

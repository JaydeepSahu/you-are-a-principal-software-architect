namespace EnterpriseAiPlatform.Policy.Contracts.Requests;

public sealed record CreatePolicyRequest(
    string Name,
    string Description,
    string ResourceType,
    string Effect,
    IReadOnlyList<string> Principals,
    IReadOnlyList<string> Actions,
    IReadOnlyList<string> Conditions,
    DateTimeOffset? ExpiresAtUtc);

public sealed record EvaluatePolicyRequest(
    string ResourceType,
    string Action,
    string? Principal = null,
    IReadOnlyDictionary<string, string>? Context = null);

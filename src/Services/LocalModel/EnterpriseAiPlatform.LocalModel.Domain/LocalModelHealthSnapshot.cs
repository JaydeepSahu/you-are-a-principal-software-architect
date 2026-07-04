namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed record LocalModelHealthSnapshot(
    string ProviderKey,
    string Name,
    LocalModelBackendKind BackendKind,
    LocalModelHealthState State,
    int ActiveRequests,
    int Capacity,
    DateTimeOffset CheckedAtUtc,
    string? Detail = null);

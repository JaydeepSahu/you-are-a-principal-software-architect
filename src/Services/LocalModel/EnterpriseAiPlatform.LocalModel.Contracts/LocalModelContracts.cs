using EnterpriseAiPlatform.LocalModel.Domain;

namespace EnterpriseAiPlatform.LocalModel.Contracts;

public sealed record LocalModelProviderUpsertRequest(
    string ProviderKey,
    string Name,
    LocalModelBackendKind BackendKind,
    string ModelName,
    string BaseUri,
    string? ChatPath = "/v1/chat/completions",
    string? StreamPath = null,
    string? HealthPath = "/health",
    string? DiscoveryPath = "/v1/models",
    string? ApiKeyHeaderName = "Authorization",
    string? ApiKey = null,
    LocalModelLoadBalancingStrategy DefaultStrategy = LocalModelLoadBalancingStrategy.RoundRobin,
    IReadOnlyList<LocalModelGpuProfile>? Gpus = null,
    int MaxConcurrency = 1,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record LocalModelProviderResponse(
    string ProviderKey,
    string Name,
    LocalModelBackendKind BackendKind,
    string ModelName,
    LocalModelEndpoint Endpoint,
    LocalModelLoadBalancingStrategy DefaultStrategy,
    IReadOnlyList<LocalModelGpuProfile> Gpus,
    LocalModelCapability Capabilities,
    int MaxConcurrency,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyDictionary<string, string> Metadata);

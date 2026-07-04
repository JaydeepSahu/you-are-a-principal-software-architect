namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed record LocalModelDescriptor(
    string ProviderKey,
    string Name,
    LocalModelBackendKind BackendKind,
    string ModelName,
    LocalModelCapability Capabilities,
    LocalModelEndpoint Endpoint,
    LocalModelLoadBalancingStrategy DefaultStrategy,
    IReadOnlyList<LocalModelGpuProfile> Gpus,
    int MaxConcurrency,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyDictionary<string, string> Metadata);

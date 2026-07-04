using EnterpriseAiPlatform.ProviderAdapters.Domain;

namespace EnterpriseAiPlatform.ProviderAdapters.Contracts;

public sealed record ProviderPluginRegistrationRequest(
    string ProviderKey,
    ProviderKind ProviderKind,
    string DisplayName,
    string DefaultModel,
    ProviderCapability Capabilities,
    Uri? Endpoint,
    ProviderHealthState Health,
    ProviderResiliencePolicy ResiliencePolicy,
    IReadOnlyList<string> Tags);

public sealed record ProviderPluginRegistrationResponse(
    string ProviderKey,
    ProviderKind ProviderKind,
    string DisplayName,
    string DefaultModel,
    ProviderCapability Capabilities,
    Uri? Endpoint,
    ProviderHealthState Health,
    ProviderResiliencePolicy ResiliencePolicy,
    IReadOnlyList<string> Tags);

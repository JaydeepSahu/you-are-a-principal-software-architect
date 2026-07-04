using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.LocalModel.Application;

public static class LocalModelErrors
{
    public static ErrorDetail ProviderNotRegistered(string? providerKey)
        => ErrorDetail.Create("local_model.provider_not_registered", $"Provider '{providerKey ?? "<unspecified>"}' is not registered.");

    public static ErrorDetail ProviderInvocationFailed(string providerKey, string reason)
        => ErrorDetail.Create("local_model.provider_invocation_failed", $"Provider '{providerKey}' failed: {reason}.");

    public static ErrorDetail DiscoveryFailed(string providerKey, string reason)
        => ErrorDetail.Create("local_model.discovery_failed", $"Discovery failed for provider '{providerKey}': {reason}.");
}

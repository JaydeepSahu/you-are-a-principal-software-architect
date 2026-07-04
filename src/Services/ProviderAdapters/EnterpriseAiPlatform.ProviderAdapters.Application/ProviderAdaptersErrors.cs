using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ProviderAdapters.Application;

public static class ProviderAdaptersErrors
{
    public static ErrorDetail NoProviderRegistered(string? providerKey)
        => ErrorDetail.Create(
            "provider_adapters.no_provider_registered",
            $"No provider adapter was registered for '{providerKey ?? "default"}'.");

    public static ErrorDetail ProviderUnavailable(string providerKey, string reason)
        => ErrorDetail.Create(
            "provider_adapters.unavailable",
            $"Provider '{providerKey}' is unavailable: {reason}");

    public static ErrorDetail ProviderInvocationFailed(string providerKey, string reason)
        => ErrorDetail.Create(
            "provider_adapters.invocation_failed",
            $"Provider '{providerKey}' invocation failed: {reason}");

    public static ErrorDetail CircuitOpen(string providerKey, TimeSpan retryAfter)
        => ErrorDetail.Create(
            "provider_adapters.circuit_open",
            $"Provider '{providerKey}' circuit is open for {retryAfter.TotalSeconds:N1}s.");
}

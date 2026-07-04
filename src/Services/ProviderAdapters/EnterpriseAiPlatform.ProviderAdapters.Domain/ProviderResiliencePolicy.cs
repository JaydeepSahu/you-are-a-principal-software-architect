namespace EnterpriseAiPlatform.ProviderAdapters.Domain;

public sealed record ProviderResiliencePolicy(
    int MaxRetries,
    TimeSpan Timeout,
    TimeSpan InitialRetryDelay,
    double BackoffMultiplier,
    int CircuitBreakerFailureThreshold,
    TimeSpan CircuitBreakerOpenDuration,
    IReadOnlyList<string>? FallbackProviderKeys = null)
{
    public static ProviderResiliencePolicy Default { get; } = new(
        MaxRetries: 2,
        Timeout: TimeSpan.FromSeconds(30),
        InitialRetryDelay: TimeSpan.FromMilliseconds(200),
        BackoffMultiplier: 2.0d,
        CircuitBreakerFailureThreshold: 3,
        CircuitBreakerOpenDuration: TimeSpan.FromSeconds(30));
}

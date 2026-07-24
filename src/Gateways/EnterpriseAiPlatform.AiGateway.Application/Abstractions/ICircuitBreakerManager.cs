using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.AiGateway.Application.Abstractions;

public enum CircuitState
{
    Closed = 0,   // Operating normally
    Open = 1,     // Failing, trips requests to fallback
    HalfOpen = 2  // Testing single probe request
}

public sealed record ProviderCircuitStatus(
    string ProviderName,
    CircuitState State,
    long SuccessCount,
    long FailureCount,
    double FailureRatePercentage,
    DateTimeOffset LastStateChangeUtc,
    string? PrimaryFallbackProvider);

public sealed record ResilientRoutingRequest(
    string PreferredProvider,
    string ModelId,
    string PromptPayload,
    string? ExplicitFallbackProvider = null);

public sealed record ResilientRoutingResult(
    string ExecutedProvider,
    bool WasFallbackUsed,
    string ResponsePayload,
    long LatencyMs,
    string CircuitStateAtExecution);

public interface ICircuitBreakerManager
{
    ProviderCircuitStatus GetProviderStatus(string providerName);
    IReadOnlyList<ProviderCircuitStatus> GetAllStatuses();
    void RecordOutcome(string providerName, bool isSuccess, bool isRateLimit = false);
    Task<Result<ResilientRoutingResult>> RouteWithResilienceAsync(
        TenantId tenantId,
        ResilientRoutingRequest request,
        Func<string, CancellationToken, Task<Result<string>>> providerExecutor,
        CancellationToken cancellationToken = default);
}

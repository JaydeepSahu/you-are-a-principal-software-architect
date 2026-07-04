using EnterpriseAiPlatform.ProviderAdapters.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ProviderAdapters.Application.Abstractions;

public interface IProviderPlugin
{
    ProviderPluginDescriptor Descriptor { get; }

    IProviderAdapter CreateAdapter(IServiceProvider serviceProvider);
}

public interface IProviderPluginRegistry
{
    void Register(IProviderPlugin plugin);

    bool TryGet(string providerKey, out IProviderPlugin plugin);

    IReadOnlyCollection<IProviderPlugin> GetAll();
}

public interface IProviderTelemetry
{
    void RecordAttempt(string providerKey, ProviderKind providerKind, bool streaming, TimeSpan elapsed, bool success, string? reason = null);

    void RecordCircuitState(string providerKey, ProviderKind providerKind, ProviderHealthState state, string? reason = null);
}

public interface IProviderCircuitBreakerStore
{
    bool IsOpen(string providerKey, out TimeSpan retryAfter);

    void RecordSuccess(string providerKey);

    void RecordFailure(string providerKey, TimeSpan openDuration, int failureThreshold);

    ProviderHealthState GetHealthState(string providerKey);
}

public interface IProviderOrchestrator
{
    Task<Result<ProviderInvocationResponse>> InvokeAsync(ProviderInvocationRequest request, ProviderInvocationContext context, CancellationToken cancellationToken = default);

    Task<Result<ProviderStreamingSession>> StreamAsync(ProviderInvocationRequest request, ProviderInvocationContext context, CancellationToken cancellationToken = default);
}

using EnterpriseAiPlatform.ProviderAdapters.Application.Abstractions;
using EnterpriseAiPlatform.ProviderAdapters.Domain;

namespace EnterpriseAiPlatform.ProviderAdapters.Infrastructure;

public sealed class InMemoryProviderTelemetry : IProviderTelemetry
{
    private readonly object _gate = new();
    private readonly Queue<ProviderTelemetrySnapshot> _recent = new();

    public IReadOnlyCollection<ProviderTelemetrySnapshot> Recent
    {
        get
        {
            lock (_gate)
            {
                return _recent.ToArray();
            }
        }
    }

    public void RecordAttempt(string providerKey, ProviderKind providerKind, bool streaming, TimeSpan elapsed, bool success, string? reason = null)
    {
        lock (_gate)
        {
            _recent.Enqueue(new ProviderTelemetrySnapshot(providerKey, providerKind, streaming, elapsed, success, reason, DateTimeOffset.UtcNow));
            while (_recent.Count > 256)
            {
                _recent.Dequeue();
            }
        }
    }

    public void RecordCircuitState(string providerKey, ProviderKind providerKind, ProviderHealthState state, string? reason = null)
    {
        lock (_gate)
        {
            _recent.Enqueue(new ProviderTelemetrySnapshot(providerKey, providerKind, false, TimeSpan.Zero, state is ProviderHealthState.Healthy or ProviderHealthState.Unknown, reason, DateTimeOffset.UtcNow));
            while (_recent.Count > 256)
            {
                _recent.Dequeue();
            }
        }
    }
}

public sealed record ProviderTelemetrySnapshot(
    string ProviderKey,
    ProviderKind ProviderKind,
    bool Streaming,
    TimeSpan Duration,
    bool Success,
    string? Reason,
    DateTimeOffset RecordedAtUtc);

using EnterpriseAiPlatform.Routing.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Domain;

namespace EnterpriseAiPlatform.Routing.Infrastructure.Telemetry;

public sealed class MemoryRoutingTelemetry : IRoutingTelemetry
{
    private readonly object _gate = new();
    private readonly List<RoutingTelemetrySnapshot> _snapshots = [];

    public IReadOnlyList<RoutingTelemetrySnapshot> Snapshots
    {
        get
        {
            lock (_gate)
            {
                return _snapshots.ToArray();
            }
        }
    }

    public void RecordEvaluation(RoutingMode mode, RoutingRequestCategory category, string selectedModelKey, TimeSpan elapsed)
    {
        lock (_gate)
        {
            _snapshots.Add(new RoutingTelemetrySnapshot(mode, category, selectedModelKey, elapsed, DateTimeOffset.UtcNow));
            if (_snapshots.Count > 500)
            {
                _snapshots.RemoveRange(0, _snapshots.Count - 500);
            }
        }
    }
}

public sealed record RoutingTelemetrySnapshot(
    RoutingMode Mode,
    RoutingRequestCategory Category,
    string SelectedModelKey,
    TimeSpan Elapsed,
    DateTimeOffset RecordedAtUtc);

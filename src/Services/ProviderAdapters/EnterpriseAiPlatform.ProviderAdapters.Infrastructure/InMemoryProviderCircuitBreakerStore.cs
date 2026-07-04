using EnterpriseAiPlatform.ProviderAdapters.Application.Abstractions;
using EnterpriseAiPlatform.ProviderAdapters.Domain;

namespace EnterpriseAiPlatform.ProviderAdapters.Infrastructure;

public sealed class InMemoryProviderCircuitBreakerStore : IProviderCircuitBreakerStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, CircuitState> _states = new(StringComparer.OrdinalIgnoreCase);

    public bool IsOpen(string providerKey, out TimeSpan retryAfter)
    {
        lock (_gate)
        {
            if (!_states.TryGetValue(providerKey, out var state))
            {
                retryAfter = TimeSpan.Zero;
                return false;
            }

            if (state.OpenUntilUtc is null)
            {
                retryAfter = TimeSpan.Zero;
                return false;
            }

            var remaining = state.OpenUntilUtc.Value - DateTimeOffset.UtcNow;
            if (remaining > TimeSpan.Zero)
            {
                retryAfter = remaining;
                return true;
            }

            state.OpenUntilUtc = null;
            retryAfter = TimeSpan.Zero;
            return false;
        }
    }

    public void RecordSuccess(string providerKey)
    {
        lock (_gate)
        {
            if (!_states.TryGetValue(providerKey, out var state))
            {
                state = new CircuitState();
                _states[providerKey] = state;
            }

            state.ConsecutiveFailures = 0;
            state.OpenUntilUtc = null;
        }
    }

    public void RecordFailure(string providerKey, TimeSpan openDuration, int failureThreshold)
    {
        lock (_gate)
        {
            if (!_states.TryGetValue(providerKey, out var state))
            {
                state = new CircuitState();
                _states[providerKey] = state;
            }

            state.ConsecutiveFailures++;
            if (state.ConsecutiveFailures >= failureThreshold)
            {
                state.OpenUntilUtc = DateTimeOffset.UtcNow + openDuration;
            }
        }
    }

    public ProviderHealthState GetHealthState(string providerKey)
    {
        lock (_gate)
        {
            if (!_states.TryGetValue(providerKey, out var state))
            {
                return ProviderHealthState.Unknown;
            }

            if (state.OpenUntilUtc is { } openUntil && openUntil > DateTimeOffset.UtcNow)
            {
                return ProviderHealthState.CircuitOpen;
            }

            return state.ConsecutiveFailures switch
            {
                0 => ProviderHealthState.Healthy,
                1 => ProviderHealthState.Degraded,
                _ => ProviderHealthState.Unhealthy,
            };
        }
    }

    private sealed class CircuitState
    {
        public int ConsecutiveFailures { get; set; }

        public DateTimeOffset? OpenUntilUtc { get; set; }
    }
}

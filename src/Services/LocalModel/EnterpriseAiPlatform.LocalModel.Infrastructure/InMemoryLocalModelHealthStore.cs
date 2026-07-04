using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Domain;

namespace EnterpriseAiPlatform.LocalModel.Infrastructure;

public sealed class InMemoryLocalModelHealthStore : ILocalModelHealthStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, State> _states = new(StringComparer.OrdinalIgnoreCase);

    public bool TryAcquire(string providerKey, int capacity, out int activeRequests)
    {
        lock (_gate)
        {
            var state = GetOrCreate(providerKey);
            if (state.ActiveRequests >= Math.Max(1, capacity))
            {
                state.Health = LocalModelHealthState.Busy;
                activeRequests = state.ActiveRequests;
                return false;
            }

            state.ActiveRequests++;
            state.Health = state.Health == LocalModelHealthState.Unknown ? LocalModelHealthState.Healthy : state.Health;
            activeRequests = state.ActiveRequests;
            return true;
        }
    }

    public void Release(string providerKey)
    {
        lock (_gate)
        {
            var state = GetOrCreate(providerKey);
            state.ActiveRequests = Math.Max(0, state.ActiveRequests - 1);
            if (state.Health == LocalModelHealthState.Busy && state.ActiveRequests < state.Capacity)
            {
                state.Health = LocalModelHealthState.Healthy;
            }
        }
    }

    public void RecordHealth(LocalModelHealthSnapshot snapshot)
    {
        lock (_gate)
        {
            _states[snapshot.ProviderKey] = new State
            {
                ActiveRequests = snapshot.ActiveRequests,
                Capacity = snapshot.Capacity,
                Health = snapshot.State,
                Snapshot = snapshot,
            };
        }
    }

    public LocalModelHealthSnapshot GetSnapshot(string providerKey)
    {
        lock (_gate)
        {
            return GetOrCreate(providerKey).Snapshot;
        }
    }

    public IReadOnlyCollection<LocalModelHealthSnapshot> GetAllSnapshots()
    {
        lock (_gate)
        {
            return _states.Values.Select(state => state.Snapshot).ToArray();
        }
    }

    private State GetOrCreate(string providerKey)
    {
        if (_states.TryGetValue(providerKey, out var state))
        {
            return state;
        }

        state = new State
        {
            Capacity = 1,
            Health = LocalModelHealthState.Unknown,
            Snapshot = new LocalModelHealthSnapshot(providerKey, providerKey, LocalModelBackendKind.OpenAICompatible, LocalModelHealthState.Unknown, 0, 1, DateTimeOffset.UtcNow, null),
        };
        _states[providerKey] = state;
        return state;
    }

    private sealed class State
    {
        public int ActiveRequests { get; set; }
        public int Capacity { get; set; }
        public LocalModelHealthState Health { get; set; }
        public LocalModelHealthSnapshot Snapshot { get; set; } = default!;
    }
}

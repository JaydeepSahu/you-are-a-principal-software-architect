using System.Collections.Concurrent;
using EnterpriseAiPlatform.Routing.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Routing.Infrastructure;

public sealed class InMemoryRoutingConfigurationRepository : IRoutingConfigurationRepository
{
    private readonly ConcurrentDictionary<Guid, RoutingConfiguration> _configurations = new();

    public Task<RoutingConfiguration?> GetAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var configuration = _configurations.Values.FirstOrDefault(item => item.TenantId == tenantId);
        return Task.FromResult(configuration);
    }

    public Task UpsertAsync(RoutingConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _configurations[configuration.Id.Value] = configuration;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        foreach (var pair in _configurations.Where(item => item.Value.TenantId == tenantId).ToArray())
        {
            _configurations.TryRemove(pair.Key, out _);
        }

        return Task.CompletedTask;
    }
}

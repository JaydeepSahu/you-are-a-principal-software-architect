using System.Collections.Concurrent;
using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ModelRegistry.Infrastructure;

public sealed class InMemoryModelRegistryRepository : IModelRegistryRepository
{
    private readonly ConcurrentDictionary<Guid, ModelRegistryEntry> _models = new();

    public Task<ModelRegistryEntry?> GetByIdAsync(
        ModelRegistryEntryId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        _models.TryGetValue(id.Value, out var model);
        return Task.FromResult(model?.TenantId == tenantId ? model : null);
    }

    public Task<ModelRegistryEntry?> GetByProviderAndNameAsync(
        TenantId tenantId,
        ModelProvider provider,
        string providerModelName,
        CancellationToken cancellationToken = default)
    {
        var model = _models.Values.FirstOrDefault(entry =>
            entry.TenantId == tenantId
            && entry.Provider == provider
            && entry.ProviderModelName.Equals(providerModelName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(model);
    }

    public Task<IReadOnlyList<ModelRegistryEntry>> GetByTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ModelRegistryEntry> models = _models.Values
            .Where(entry => entry.TenantId == tenantId)
            .OrderByDescending(entry => entry.UpdatedAtUtc)
            .ToList();
        return Task.FromResult(models);
    }

    public Task UpsertAsync(ModelRegistryEntry entry, CancellationToken cancellationToken = default)
    {
        _models[entry.Id.Value] = entry;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(
        ModelRegistryEntryId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        if (!_models.TryGetValue(id.Value, out var model) || model.TenantId != tenantId)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(_models.TryRemove(id.Value, out _));
    }
}

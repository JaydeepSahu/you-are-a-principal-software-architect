using EnterpriseAiPlatform.ModelRegistry.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;

public interface IModelRegistryRepository
{
    Task<ModelRegistryEntry?> GetByIdAsync(
        ModelRegistryEntryId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    Task<ModelRegistryEntry?> GetByProviderAndNameAsync(
        TenantId tenantId,
        ModelProvider provider,
        string providerModelName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModelRegistryEntry>> GetByTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(ModelRegistryEntry entry, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        ModelRegistryEntryId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}

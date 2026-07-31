using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAiPlatform.ModelRegistry.Infrastructure.Persistence;

public sealed class EfCoreModelRegistryRepository(ModelRegistryDbContext dbContext) : IModelRegistryRepository
{
    public async Task<ModelRegistryEntry?> GetByIdAsync(
        ModelRegistryEntryId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ModelEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId, cancellationToken);
    }

    public async Task<ModelRegistryEntry?> GetByProviderAndNameAsync(
        TenantId tenantId,
        ModelProvider provider,
        string providerModelName,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ModelEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.TenantId == tenantId
                && m.Provider == provider
                && m.ProviderModelName.Equals(providerModelName, StringComparison.OrdinalIgnoreCase), cancellationToken);
    }

    public async Task<IReadOnlyList<ModelRegistryEntry>> GetByTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ModelEntries
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId)
            .OrderByDescending(m => m.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(ModelRegistryEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var existing = await dbContext.ModelEntries
            .FirstOrDefaultAsync(m => m.Id == entry.Id, cancellationToken);

        if (existing is null)
        {
            await dbContext.ModelEntries.AddAsync(entry, cancellationToken);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(entry);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        ModelRegistryEntryId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.ModelEntries
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        dbContext.ModelEntries.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}

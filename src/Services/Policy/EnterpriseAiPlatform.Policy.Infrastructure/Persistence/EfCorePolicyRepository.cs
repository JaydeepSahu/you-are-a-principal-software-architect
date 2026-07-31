using EnterpriseAiPlatform.Policy.Application.Abstractions;
using EnterpriseAiPlatform.Policy.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using PolicyEntity = EnterpriseAiPlatform.Policy.Domain.Policy;

namespace EnterpriseAiPlatform.Policy.Infrastructure.Persistence;

public sealed class EfCorePolicyRepository(PolicyDbContext dbContext) : IPolicyRepository
{
    public async Task<PolicyEntity?> GetByIdAsync(PolicyId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<PolicyEntity>> QueryAsync(
        TenantId tenantId,
        PolicyResourceType? resourceType,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Policies
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (resourceType.HasValue)
        {
            query = query.Where(p => p.ResourceType == resourceType.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PolicyEntity policy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        await dbContext.Policies.AddAsync(policy, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _dbContext;

    public TenantRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tenants.FirstOrDefaultAsync(tenant => tenant.Id == tenantId, cancellationToken);
    }

    public Task<Tenant?> GetByEntraTenantIdAsync(Guid entraTenantId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tenants.FirstOrDefaultAsync(
            tenant => tenant.EntraTenantId == entraTenantId,
            cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }
}

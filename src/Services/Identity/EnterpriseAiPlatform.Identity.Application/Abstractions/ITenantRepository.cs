using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    Task<Tenant?> GetByEntraTenantIdAsync(Guid entraTenantId, CancellationToken cancellationToken = default);

    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
}

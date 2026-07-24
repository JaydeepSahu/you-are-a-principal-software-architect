using EnterpriseAiPlatform.Policy.Domain;

namespace EnterpriseAiPlatform.Policy.Application.Abstractions;

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(PolicyId id, SharedKernel.TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Policy>> QueryAsync(SharedKernel.TenantId tenantId, PolicyResourceType? resourceType, CancellationToken cancellationToken = default);
    Task AddAsync(Policy policy, CancellationToken cancellationToken = default);
}

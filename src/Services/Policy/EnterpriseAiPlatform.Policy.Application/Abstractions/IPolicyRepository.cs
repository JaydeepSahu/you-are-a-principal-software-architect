using PolicyEntity = EnterpriseAiPlatform.Policy.Domain.Policy;
using EnterpriseAiPlatform.Policy.Domain;

namespace EnterpriseAiPlatform.Policy.Application.Abstractions;

public interface IPolicyRepository
{
    Task<PolicyEntity?> GetByIdAsync(PolicyId id, SharedKernel.TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyEntity>> QueryAsync(SharedKernel.TenantId tenantId, PolicyResourceType? resourceType, CancellationToken cancellationToken = default);
    Task AddAsync(PolicyEntity policy, CancellationToken cancellationToken = default);
}

using EnterpriseAiPlatform.PromptIntelligence.Domain;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;

public interface IProfileRepository
{
    Task<PromptOptimizationProfile?> GetByIdAsync(
        PromptOptimizationProfileId id,
        SharedKernel.TenantId tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptOptimizationProfile>> GetByTenantAsync(
        SharedKernel.TenantId tenantId,
        CancellationToken cancellationToken = default);

    Task AddAsync(PromptOptimizationProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(PromptOptimizationProfile profile, CancellationToken cancellationToken = default);
}

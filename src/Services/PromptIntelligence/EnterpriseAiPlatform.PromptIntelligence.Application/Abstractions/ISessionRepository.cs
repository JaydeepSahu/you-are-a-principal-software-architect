using EnterpriseAiPlatform.PromptIntelligence.Domain;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;

public interface ISessionRepository
{
    Task<PromptOptimizationSession?> GetByIdAsync(
        PromptOptimizationSessionId id,
        SharedKernel.TenantId tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptOptimizationSession>> GetByTenantAsync(
        SharedKernel.TenantId tenantId,
        int take = 50,
        int skip = 0,
        CancellationToken cancellationToken = default);

    Task AddAsync(PromptOptimizationSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(PromptOptimizationSession session, CancellationToken cancellationToken = default);
}

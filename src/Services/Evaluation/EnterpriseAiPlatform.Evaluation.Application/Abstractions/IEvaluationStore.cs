using EvaluationDomain = EnterpriseAiPlatform.Evaluation.Domain;
using SharedKernel = EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Evaluation.Application.Abstractions;

public interface IEvaluationStore
{
    Task<EvaluationDomain.Evaluation?> GetByIdAsync(EvaluationDomain.EvaluationId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EvaluationDomain.Evaluation>> ListAsync(SharedKernel.TenantId tenantId, string? targetId, string? targetType, DateTimeOffset? from, DateTimeOffset? toDate, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(SharedKernel.TenantId tenantId, string? targetId, string? targetType, DateTimeOffset? from, DateTimeOffset? toDate, CancellationToken cancellationToken = default);
    Task AddAsync(EvaluationDomain.Evaluation evaluation, CancellationToken cancellationToken = default);
}

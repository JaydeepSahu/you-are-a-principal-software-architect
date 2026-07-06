using System.Collections.Concurrent;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Evaluation.Application.Abstractions;
using EvaluationDomain = EnterpriseAiPlatform.Evaluation.Domain;
using SharedKernel = EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Evaluation.Infrastructure;

public sealed class InMemoryEvaluationStore : IEvaluationStore
{
    private readonly ConcurrentDictionary<EvaluationDomain.EvaluationId, EvaluationDomain.Evaluation> _store = new();

    public Task<EvaluationDomain.Evaluation?> GetByIdAsync(EvaluationDomain.EvaluationId id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var evaluation);
        return Task.FromResult(evaluation);
    }

    public Task<IReadOnlyList<EvaluationDomain.Evaluation>> ListAsync(
        SharedKernel.TenantId tenantId,
        string? targetId,
        string? targetType,
        DateTimeOffset? from,
        DateTimeOffset? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _store.Values
            .Where(e => e.TenantId == tenantId)
            .Where(e => string.IsNullOrEmpty(targetId) || e.TargetId.Contains(targetId, StringComparison.OrdinalIgnoreCase))
            .Where(e => string.IsNullOrEmpty(targetType) || e.TargetType.Equals(targetType, StringComparison.OrdinalIgnoreCase))
            .Where(e => !from.HasValue || e.EvaluatedAtUtc >= from.Value)
            .Where(e => !toDate.HasValue || e.EvaluatedAtUtc <= toDate.Value)
            .OrderByDescending(e => e.EvaluatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<EvaluationDomain.Evaluation>>(query);
    }

    public Task<int> CountAsync(
        SharedKernel.TenantId tenantId,
        string? targetId,
        string? targetType,
        DateTimeOffset? from,
        DateTimeOffset? toDate,
        CancellationToken cancellationToken = default)
    {
        var count = _store.Values
            .Count(e => e.TenantId == tenantId
                && (string.IsNullOrEmpty(targetId) || e.TargetId.Contains(targetId, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrEmpty(targetType) || e.TargetType.Equals(targetType, StringComparison.OrdinalIgnoreCase))
                && (!from.HasValue || e.EvaluatedAtUtc >= from.Value)
                && (!toDate.HasValue || e.EvaluatedAtUtc <= toDate.Value));

        return Task.FromResult(count);
    }

    public Task AddAsync(EvaluationDomain.Evaluation evaluation, CancellationToken cancellationToken = default)
    {
        _store.TryAdd(evaluation.Id, evaluation);
        return Task.CompletedTask;
    }
}

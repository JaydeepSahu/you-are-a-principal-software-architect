using EnterpriseAiPlatform.Observability.Domain;

namespace EnterpriseAiPlatform.Observability.Application.Abstractions;

public interface ITraceRepository
{
    Task AddAsync(Trace trace, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Trace>> QueryAsync(
        SharedKernel.TenantId tenantId,
        string? traceId,
        string? service,
        TraceSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? until,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        SharedKernel.TenantId tenantId,
        string? traceId,
        string? service,
        TraceSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? until,
        CancellationToken cancellationToken = default);
}

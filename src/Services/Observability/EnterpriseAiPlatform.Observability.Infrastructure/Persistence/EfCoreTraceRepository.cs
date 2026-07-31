using EnterpriseAiPlatform.Observability.Application.Abstractions;
using EnterpriseAiPlatform.Observability.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAiPlatform.Observability.Infrastructure.Persistence;

public sealed class EfCoreTraceRepository(ObservabilityDbContext dbContext) : ITraceRepository
{
    public async Task AddAsync(Trace trace, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trace);
        await dbContext.Traces.AddAsync(trace, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Trace>> QueryAsync(
        TenantId tenantId,
        string? traceId,
        string? service,
        TraceSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? until,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Traces
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            query = query.Where(t => t.TraceIdValue == traceId);
        }

        if (!string.IsNullOrWhiteSpace(service))
        {
            query = query.Where(t => t.Service == service);
        }

        if (minSeverity.HasValue)
        {
            query = query.Where(t => t.Severity >= minSeverity.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(t => t.StartedAtUtc >= from.Value);
        }

        if (until.HasValue)
        {
            query = query.Where(t => t.StartedAtUtc <= until.Value);
        }

        return await query
            .OrderByDescending(t => t.StartedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        TenantId tenantId,
        string? traceId,
        string? service,
        TraceSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? until,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Traces
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            query = query.Where(t => t.TraceIdValue == traceId);
        }

        if (!string.IsNullOrWhiteSpace(service))
        {
            query = query.Where(t => t.Service == service);
        }

        if (minSeverity.HasValue)
        {
            query = query.Where(t => t.Severity >= minSeverity.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(t => t.StartedAtUtc >= from.Value);
        }

        if (until.HasValue)
        {
            query = query.Where(t => t.StartedAtUtc <= until.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}

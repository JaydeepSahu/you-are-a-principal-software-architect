using EnterpriseAiPlatform.Audit.Application.Abstractions;
using EnterpriseAiPlatform.Audit.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAiPlatform.Audit.Infrastructure.Persistence;

public sealed class EfCoreAuditRepository(AuditDbContext dbContext) : IAuditRepository
{
    public async Task<IReadOnlyList<AuditEntry>> QueryAsync(
        TenantId tenantId,
        string? resourceType,
        string? resourceId,
        string? userId,
        AuditAction? action,
        AuditSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? until,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditEntries
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(resourceType))
        {
            query = query.Where(e => e.ResourceType == resourceType);
        }

        if (!string.IsNullOrWhiteSpace(resourceId))
        {
            query = query.Where(e => e.ResourceId == resourceId);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(e => e.UserId == userId);
        }

        if (action.HasValue)
        {
            query = query.Where(e => e.Action == action.Value);
        }

        if (minSeverity.HasValue)
        {
            query = query.Where(e => e.Severity >= minSeverity.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(e => e.OccurredAtUtc >= from.Value);
        }

        if (until.HasValue)
        {
            query = query.Where(e => e.OccurredAtUtc <= until.Value);
        }

        return await query
            .OrderByDescending(e => e.OccurredAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        TenantId tenantId,
        string? resourceType,
        string? resourceId,
        string? userId,
        AuditAction? action,
        AuditSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? until,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditEntries
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(resourceType))
        {
            query = query.Where(e => e.ResourceType == resourceType);
        }

        if (!string.IsNullOrWhiteSpace(resourceId))
        {
            query = query.Where(e => e.ResourceId == resourceId);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(e => e.UserId == userId);
        }

        if (action.HasValue)
        {
            query = query.Where(e => e.Action == action.Value);
        }

        if (minSeverity.HasValue)
        {
            query = query.Where(e => e.Severity >= minSeverity.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(e => e.OccurredAtUtc >= from.Value);
        }

        if (until.HasValue)
        {
            query = query.Where(e => e.OccurredAtUtc <= until.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        await dbContext.AuditEntries.AddAsync(entry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

using System.Collections.Concurrent;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Audit.Application.Abstractions;
using EnterpriseAiPlatform.Audit.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Audit.Infrastructure;

public sealed class InMemoryAuditRepository : IAuditRepository
{
    private readonly ConcurrentDictionary<Guid, AuditEntry> _entries = new();

    public Task<IReadOnlyList<AuditEntry>> QueryAsync(
        TenantId tenantId,
        string? resourceType,
        string? resourceId,
        string? userId,
        AuditAction? action,
        AuditSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _entries.Values
            .Where(e => e.TenantId == tenantId)
            .Where(e => resourceType is null || e.ResourceType == resourceType)
            .Where(e => resourceId is null || e.ResourceId == resourceId)
            .Where(e => userId is null || e.UserId == userId)
            .Where(e => action is null || e.Action == action)
            .Where(e => minSeverity is null || e.Severity >= minSeverity)
            .Where(e => from is null || e.OccurredAtUtc >= from)
            .Where(e => to is null || e.OccurredAtUtc <= to)
            .OrderByDescending(e => e.OccurredAtUtc);

        var results = query.Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<AuditEntry>>(results);
    }

    public Task<int> CountAsync(
        TenantId tenantId,
        string? resourceType,
        string? resourceId,
        string? userId,
        AuditAction? action,
        AuditSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var count = _entries.Values
            .Count(e => e.TenantId == tenantId
                && (resourceType is null || e.ResourceType == resourceType)
                && (resourceId is null || e.ResourceId == resourceId)
                && (userId is null || e.UserId == userId)
                && (action is null || e.Action == action)
                && (minSeverity is null || e.Severity >= minSeverity)
                && (from is null || e.OccurredAtUtc >= from)
                && (to is null || e.OccurredAtUtc <= to));
        return Task.FromResult(count);
    }

    public Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        _entries[entry.Id.Value] = entry;
        return Task.CompletedTask;
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddAuditInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<IAuditRepository, InMemoryAuditRepository>();

        return services;
    }
}

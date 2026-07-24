using EnterpriseAiPlatform.Audit.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Audit.Application.Abstractions;

public interface IAuditRepository
{
    Task<IReadOnlyList<AuditEntry>> QueryAsync(
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
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        TenantId tenantId,
        string? resourceType,
        string? resourceId,
        string? userId,
        AuditAction? action,
        AuditSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? until,
        CancellationToken cancellationToken = default);

    Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}

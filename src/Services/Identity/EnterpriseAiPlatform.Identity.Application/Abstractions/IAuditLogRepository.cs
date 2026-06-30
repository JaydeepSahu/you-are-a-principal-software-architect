using EnterpriseAiPlatform.Identity.Domain;

namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry auditLogEntry, CancellationToken cancellationToken = default);
}

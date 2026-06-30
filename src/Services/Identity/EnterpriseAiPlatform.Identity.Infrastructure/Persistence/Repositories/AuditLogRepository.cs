using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Domain;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly IdentityDbContext _dbContext;

    public AuditLogRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditLogEntry auditLogEntry, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogEntries.AddAsync(auditLogEntry, cancellationToken);
    }
}

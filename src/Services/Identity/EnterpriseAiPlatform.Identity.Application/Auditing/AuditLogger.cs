using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Domain;

namespace EnterpriseAiPlatform.Identity.Application.Auditing;

public sealed class AuditLogger : IAuditLogger
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly TimeProvider _timeProvider;

    public AuditLogger(IAuditLogRepository auditLogRepository, TimeProvider timeProvider)
    {
        _auditLogRepository = auditLogRepository;
        _timeProvider = timeProvider;
    }

    public async Task LogAsync(AuditLogRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        AuditLogEntry auditLogEntry = AuditLogEntry.Create(
            request.TenantId,
            request.Action,
            request.Outcome,
            request.SubjectId,
            request.ApplicationId,
            request.IpAddress,
            request.UserAgent,
            request.CorrelationId,
            _timeProvider.GetUtcNow());

        await _auditLogRepository.AddAsync(auditLogEntry, cancellationToken);
    }
}

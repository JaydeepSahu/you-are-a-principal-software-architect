using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Domain;

public sealed class AuditLogEntry : Entity<Guid>
{
    private AuditLogEntry()
        : base(Guid.NewGuid())
    {
        Action = string.Empty;
        Outcome = string.Empty;
        CorrelationId = string.Empty;
    }

    private AuditLogEntry(
        Guid id,
        TenantId? tenantId,
        string action,
        string outcome,
        string? subjectId,
        string? applicationId,
        string? ipAddress,
        string? userAgent,
        string correlationId,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Action = action;
        Outcome = outcome;
        SubjectId = subjectId;
        ApplicationId = applicationId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CorrelationId = correlationId;
        OccurredAtUtc = occurredAtUtc;
    }

    public TenantId? TenantId { get; private set; }

    public string Action { get; private set; }

    public string Outcome { get; private set; }

    public string? SubjectId { get; private set; }

    public string? ApplicationId { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public string CorrelationId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public static AuditLogEntry Create(
        TenantId? tenantId,
        string action,
        string outcome,
        string? subjectId,
        string? applicationId,
        string? ipAddress,
        string? userAgent,
        string correlationId,
        DateTimeOffset occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Audit action is required.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(outcome))
        {
            throw new ArgumentException("Audit outcome is required.", nameof(outcome));
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation identifier is required.", nameof(correlationId));
        }

        return new AuditLogEntry(
            Guid.NewGuid(),
            tenantId,
            action.Trim(),
            outcome.Trim(),
            subjectId,
            applicationId,
            ipAddress,
            userAgent,
            correlationId,
            occurredAtUtc);
    }
}

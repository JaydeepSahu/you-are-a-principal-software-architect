using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Audit.Domain;

public sealed class AuditEntry : Entity<AuditEntryId>
{
    internal AuditEntry(
        AuditEntryId id,
        TenantId tenantId,
        AuditAction action,
        AuditSeverity severity,
        string resourceType,
        string resourceId,
        string? userId,
        string? applicationId,
        string? correlationId,
        string? ipAddress,
        IReadOnlyDictionary<string, string> metadata,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Action = action;
        Severity = severity;
        ResourceType = resourceType;
        ResourceId = resourceId;
        UserId = userId;
        ApplicationId = applicationId;
        CorrelationId = correlationId;
        IpAddress = ipAddress;
        Metadata = metadata;
        OccurredAtUtc = occurredAtUtc;
    }

    public TenantId TenantId { get; }
    public AuditAction Action { get; }
    public AuditSeverity Severity { get; }
    public string ResourceType { get; }
    public string ResourceId { get; }
    public string? UserId { get; }
    public string? ApplicationId { get; }
    public string? CorrelationId { get; }
    public string? IpAddress { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}

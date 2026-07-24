namespace EnterpriseAiPlatform.Audit.Contracts.Responses;

public sealed record AuditEventRecordedResponse(
    Guid EventId,
    DateTimeOffset RecordedAtUtc);

public sealed record AuditLogResponse(
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<AuditEntryResponse> Entries);

public sealed record AuditEntryResponse(
    Guid Id,
    string Action,
    string Severity,
    string ResourceType,
    string ResourceId,
    string? UserId,
    string? ApplicationId,
    string? CorrelationId,
    string? IpAddress,
    IReadOnlyDictionary<string, string> Metadata,
    DateTimeOffset OccurredAtUtc);

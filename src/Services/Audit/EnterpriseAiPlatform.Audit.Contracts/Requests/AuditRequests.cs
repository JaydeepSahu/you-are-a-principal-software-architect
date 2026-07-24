namespace EnterpriseAiPlatform.Audit.Contracts.Requests;

public sealed record RecordAuditEventRequest(
    string Action,
    string Severity,
    string ResourceType,
    string ResourceId,
    string? UserId = null,
    string? ApplicationId = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record QueryAuditLogRequest(
    string? ResourceType = null,
    string? ResourceId = null,
    string? UserId = null,
    string? Action = null,
    string? MinSeverity = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Page = 1,
    int PageSize = 50);

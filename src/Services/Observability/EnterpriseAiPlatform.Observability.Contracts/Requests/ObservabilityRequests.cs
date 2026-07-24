namespace EnterpriseAiPlatform.Observability.Contracts.Requests;

public sealed record RecordTraceRequest(
    string TraceId,
    string? SpanId,
    string Name,
    string Severity,
    string Service,
    IReadOnlyDictionary<string, string>? Attributes,
    long DurationMs,
    string? ErrorMessage);

public sealed record QueryTracesRequest(
    string? TraceId = null,
    string? Service = null,
    string? MinSeverity = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Page = 1,
    int PageSize = 100);

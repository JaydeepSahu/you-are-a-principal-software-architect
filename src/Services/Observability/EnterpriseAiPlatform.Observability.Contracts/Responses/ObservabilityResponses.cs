namespace EnterpriseAiPlatform.Observability.Contracts.Responses;

public sealed record TraceRecordedResponse(Guid TraceRecordId, DateTimeOffset RecordedAtUtc);

public sealed record TraceListResponse(
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<TraceResponse> Traces);

public sealed record TraceResponse(
    Guid Id,
    string TraceId,
    string? SpanId,
    string Name,
    string Severity,
    string Service,
    IReadOnlyDictionary<string, string> Attributes,
    long DurationMs,
    bool IsError,
    DateTimeOffset StartedAtUtc);

public sealed record HealthSummaryResponse(
    int TotalTraces,
    int ErrorCount,
    double ErrorRate,
    double P50LatencyMs,
    double P95LatencyMs,
    double P99LatencyMs,
    DateTimeOffset CheckedAtUtc);

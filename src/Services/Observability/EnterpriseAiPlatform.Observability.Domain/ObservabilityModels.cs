namespace EnterpriseAiPlatform.Observability.Domain;

public sealed record TraceId
{
    private TraceId(string value) => Value = value;
    public string Value { get; }
    public static TraceId New() => new(Guid.NewGuid().ToString("N"));
    public static TraceId From(string value)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Trace identifier cannot be empty.", nameof(value))
            : new(value);
}

public sealed record SpanId
{
    private SpanId(string value) => Value = value;
    public string Value { get; }
    public static SpanId New() => new(Guid.NewGuid().ToString("N")[..16]);
    public static SpanId From(string value)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Span identifier cannot be empty.", nameof(value))
            : new(value);
}

public enum TraceSeverity
{
    Unspecified,
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Critical,
}

public sealed record Trace : SharedKernel.Entity<SharedKernel.Entity<Guid>>
{
    public Trace(
        Guid id,
        SharedKernel.TenantId tenantId,
        string traceId,
        string? spanId,
        string name,
        TraceSeverity severity,
        string service,
        IReadOnlyDictionary<string, string> attributes,
        DateTimeOffset startedAtUtc,
        long durationMs,
        string? errorMessage)
        : base(id)
    {
        TenantId = tenantId;
        TraceIdValue = traceId;
        SpanIdValue = spanId;
        Name = name;
        Severity = severity;
        Service = service;
        Attributes = attributes;
        StartedAtUtc = startedAtUtc;
        DurationMs = durationMs;
        ErrorMessage = errorMessage;
    }

    public SharedKernel.TenantId TenantId { get; }
    public string TraceIdValue { get; }
    public string? SpanIdValue { get; }
    public string Name { get; }
    public TraceSeverity Severity { get; }
    public string Service { get; }
    public IReadOnlyDictionary<string, string> Attributes { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public long DurationMs { get; }
    public string? ErrorMessage { get; }
    public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
}

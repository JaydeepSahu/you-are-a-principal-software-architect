namespace EnterpriseAiPlatform.Metering.Domain;

public sealed record MeteringRecordId
{
    private MeteringRecordId(Guid value) => Value = value;
    public Guid Value { get; }
    public static MeteringRecordId New() => new(Guid.NewGuid());
    public static MeteringRecordId From(Guid value)
        => value == Guid.Empty
            ? throw new ArgumentException("Metering record identifier cannot be empty.", nameof(value))
            : new(value);
}

public enum MeteringDimension
{
    TokenCompletion,
    TokenPrompt,
    RequestCount,
    ErrorCount,
    LatencyMs,
    CacheHit,
    CacheMiss,
}

public sealed record MeteringRecord : SharedKernel.Entity<MeteringRecordId>
{
    public MeteringRecord(
        MeteringRecordId id,
        SharedKernel.TenantId tenantId,
        string provider,
        string model,
        MeteringDimension dimension,
        double value,
        DateTimeOffset recordedAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Provider = provider;
        Model = model;
        Dimension = dimension;
        Value = value;
        RecordedAtUtc = recordedAtUtc;
    }

    public SharedKernel.TenantId TenantId { get; }
    public string Provider { get; }
    public string Model { get; }
    public MeteringDimension Dimension { get; }
    public double Value { get; }
    public DateTimeOffset RecordedAtUtc { get; }
}

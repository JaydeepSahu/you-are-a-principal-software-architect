namespace EnterpriseAiPlatform.Metering.Domain;

public sealed record MeteringUsageReport : SharedKernel.Entity<MeteringRecordId>
{
    public MeteringUsageReport(
        MeteringRecordId id,
        SharedKernel.TenantId tenantId,
        string Provider,
        string Model,
        long TokenCompletionTotal,
        long TokenPromptTotal,
        long RequestCountTotal,
        long ErrorCountTotal,
        double CostUsd,
        DateTimeOffset PeriodStartUtc,
        DateTimeOffset PeriodEndUtc,
        DateTimeOffset GeneratedAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Provider = Provider;
        Model = Model;
        TokenCompletionTotal = TokenCompletionTotal;
        TokenPromptTotal = TokenPromptTotal;
        RequestCountTotal = RequestCountTotal;
        ErrorCountTotal = ErrorCountTotal;
        CostUsd = CostUsd;
        PeriodStartUtc = PeriodStartUtc;
        PeriodEndUtc = PeriodEndUtc;
        GeneratedAtUtc = GeneratedAtUtc;
    }

    public SharedKernel.TenantId TenantId { get; }
    public string Provider { get; }
    public string Model { get; }
    public long TokenCompletionTotal { get; }
    public long TokenPromptTotal { get; }
    public long RequestCountTotal { get; }
    public long ErrorCountTotal { get; }
    public double CostUsd { get; }
    public DateTimeOffset PeriodStartUtc { get; }
    public DateTimeOffset PeriodEndUtc { get; }
    public DateTimeOffset GeneratedAtUtc { get; }
}

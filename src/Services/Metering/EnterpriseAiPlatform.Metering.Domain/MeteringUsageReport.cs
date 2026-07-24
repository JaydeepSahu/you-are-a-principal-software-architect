namespace EnterpriseAiPlatform.Metering.Domain;

public sealed class MeteringUsageReport : SharedKernel.Entity<MeteringRecordId>
{
    public MeteringUsageReport(
        MeteringRecordId id,
        SharedKernel.TenantId tenantId,
        string provider,
        string model,
        long tokenCompletionTotal,
        long tokenPromptTotal,
        long requestCountTotal,
        long errorCountTotal,
        double costUsd,
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndUtc,
        DateTimeOffset generatedAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Provider = provider;
        Model = model;
        TokenCompletionTotal = tokenCompletionTotal;
        TokenPromptTotal = tokenPromptTotal;
        RequestCountTotal = requestCountTotal;
        ErrorCountTotal = errorCountTotal;
        CostUsd = costUsd;
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
        GeneratedAtUtc = generatedAtUtc;
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

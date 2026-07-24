using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Domain;

public sealed class CostRecord : Entity<CostRecordId>
{
    public TenantId TenantId { get; }
    public string UserId { get; }
    public string? DepartmentId { get; }
    public string ProviderId { get; }
    public string ModelId { get; }
    public CostCategory Category { get; }
    public CostAmount Amount { get; }
    public int InputTokens { get; }
    public int OutputTokens { get; }
    public double? ProcessingSeconds { get; }
    public string RequestId { get; }
    public string? CorrelationId { get; }
    public DateTimeOffset RecordedAt { get; }
    public int PeriodYear { get; }
    public int PeriodMonth { get; }
    public int? PeriodDay { get; }
    public Dictionary<string, string> Metadata { get; }

    internal CostRecord(
        CostRecordId id,
        TenantId tenantId,
        string userId,
        string? departmentId,
        string providerId,
        string modelId,
        CostCategory category,
        CostAmount amount,
        int inputTokens,
        int outputTokens,
        double? processingSeconds,
        string requestId,
        string? correlationId,
        DateTimeOffset recordedAt,
        int periodYear,
        int periodMonth,
        int? periodDay,
        Dictionary<string, string> metadata)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        DepartmentId = departmentId;
        ProviderId = providerId;
        ModelId = modelId;
        Category = category;
        Amount = amount;
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        ProcessingSeconds = processingSeconds;
        RequestId = requestId;
        CorrelationId = correlationId;
        RecordedAt = recordedAt;
        PeriodYear = periodYear;
        PeriodMonth = periodMonth;
        PeriodDay = periodDay;
        Metadata = metadata;
    }

    public static CostRecord Create(
        TenantId tenantId,
        string userId,
        string? departmentId,
        string providerId,
        string modelId,
        CostCategory category,
        CostAmount amount,
        int inputTokens,
        int outputTokens,
        double? processingSeconds,
        string requestId,
        string? correlationId,
        Dictionary<string, string>? metadata = null)
    {
        var recordedAt = DateTimeOffset.UtcNow;
        return new CostRecord(
            CostRecordId.Create(),
            tenantId,
            userId,
            departmentId,
            providerId,
            modelId,
            category,
            amount,
            inputTokens,
            outputTokens,
            processingSeconds,
            requestId,
            correlationId,
            recordedAt,
            recordedAt.Year,
            recordedAt.Month,
            recordedAt.Day,
            metadata ?? new Dictionary<string, string>());
    }

    public int TotalTokens => InputTokens + OutputTokens;
}

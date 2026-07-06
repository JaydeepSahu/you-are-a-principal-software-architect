namespace EnterpriseAiPlatform.Evaluation.Domain;

public sealed class Evaluation : SharedKernel.AggregateRoot<EvaluationId>
{
    public Evaluation(
        EvaluationId id,
        SharedKernel.TenantId tenantId,
        string targetId,
        string targetType,
        EvaluationMetrics metrics,
        string? prompt = null,
        string? expectedOutput = null,
        string? actualOutput = null)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetType);
        ArgumentNullException.ThrowIfNull(metrics);

        TenantId = tenantId;
        TargetId = targetId.Trim();
        TargetType = targetType.Trim();
        Prompt = prompt?.Trim();
        ExpectedOutput = expectedOutput?.Trim();
        ActualOutput = actualOutput?.Trim();
        Metrics = metrics;
        EvaluatedAtUtc = DateTimeOffset.UtcNow;
    }

    public SharedKernel.TenantId TenantId { get; }

    public string TargetId { get; }

    public string TargetType { get; }

    public string? Prompt { get; }

    public string? ExpectedOutput { get; }

    public string? ActualOutput { get; }

    public EvaluationMetrics Metrics { get; }

    public DateTimeOffset EvaluatedAtUtc { get; }
}

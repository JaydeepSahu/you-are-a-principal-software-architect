namespace EnterpriseAiPlatform.PromptIntelligence.Domain;

public sealed class PromptOptimizationSession : SharedKernel.AggregateRoot<PromptOptimizationSessionId>
{
    public SharedKernel.TenantId TenantId { get; }
    public PromptOptimizationProfileId? ProfileId { get; }
    public string OriginalPrompt { get; }
    public OptimizationRule Rule { get; }
    public OptimizationStatus Status { get; private set; }
    public OptimizationResult? Result { get; private set; }
    public string? ErrorReason { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public PromptOptimizationSession(
        PromptOptimizationSessionId id,
        SharedKernel.TenantId tenantId,
        string originalPrompt,
        OptimizationRule rule,
        PromptOptimizationProfileId? profileId = null)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalPrompt);
        ArgumentNullException.ThrowIfNull(rule);

        TenantId = tenantId;
        ProfileId = profileId;
        OriginalPrompt = originalPrompt;
        Rule = rule;
        Status = OptimizationStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Start() => Status = OptimizationStatus.InProgress;

    public void Complete(OptimizationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Result = result;
        Status = OptimizationStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new SessionCompletedDomainEvent(Id, TenantId, result.TokenReductionPercent));
    }

    public void Fail(string reason)
    {
        ErrorReason = reason ?? "Unknown error";
        Status = OptimizationStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}

public sealed record SessionCompletedDomainEvent(
    PromptOptimizationSessionId SessionId,
    SharedKernel.TenantId TenantId,
    double TokenReductionPercent) : SharedKernel.DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);

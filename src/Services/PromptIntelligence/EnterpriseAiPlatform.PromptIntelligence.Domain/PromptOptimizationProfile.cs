namespace EnterpriseAiPlatform.PromptIntelligence.Domain;

public sealed class PromptOptimizationProfile : SharedKernel.AggregateRoot<PromptOptimizationProfileId>
{
    public SharedKernel.TenantId TenantId { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public OptimizationRule DefaultRule { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public PromptOptimizationProfile(
        PromptOptimizationProfileId id,
        SharedKernel.TenantId tenantId,
        string name,
        OptimizationRule defaultRule,
        string? description = null)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(defaultRule);

        TenantId = tenantId;
        Name = name;
        Description = description;
        DefaultRule = defaultRule;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void UpdateDetails(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ProfileUpdatedDomainEvent(Id, TenantId, UpdatedAt));
    }

    public void UpdateDefaultRule(OptimizationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        DefaultRule = rule;
        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ProfileUpdatedDomainEvent(Id, TenantId, UpdatedAt));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ProfileUpdatedDomainEvent(Id, TenantId, UpdatedAt));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ProfileUpdatedDomainEvent(Id, TenantId, UpdatedAt));
    }
}

public sealed record ProfileUpdatedDomainEvent(
    PromptOptimizationProfileId ProfileId,
    SharedKernel.TenantId TenantId,
    DateTimeOffset UpdatedAt) : SharedKernel.DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);

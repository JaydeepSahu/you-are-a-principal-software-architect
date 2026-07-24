namespace EnterpriseAiPlatform.Policy.Domain;

public sealed record PolicyId
{
    private PolicyId(Guid value) => Value = value;
    public Guid Value { get; }
    public static PolicyId New() => new(Guid.NewGuid());
    public static PolicyId From(Guid value)
        => value == Guid.Empty
            ? throw new ArgumentException("Policy identifier cannot be empty.", nameof(value))
            : new(value);
}

public enum PolicyEffect
{
    Allow,
    Deny,
}

public enum PolicyResourceType
{
    AiGateway,
    Model,
    Provider,
    Tenant,
    Token,
    RoutingRule,
    AuditLog,
    MeteringRecord,
    VectorIndex,
    KnowledgeBase,
}

public sealed class Policy : SharedKernel.Entity<PolicyId>
{
    public Policy(
        PolicyId id,
        SharedKernel.TenantId tenantId,
        string name,
        string description,
        PolicyResourceType resourceType,
        PolicyEffect effect,
        IReadOnlyList<string> principals,
        IReadOnlyList<string> actions,
        IReadOnlyList<string> conditions,
        bool isEnabled,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? expiresAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ResourceType = resourceType;
        Effect = effect;
        Principals = principals;
        Actions = actions;
        Conditions = conditions;
        IsEnabled = isEnabled;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public SharedKernel.TenantId TenantId { get; }
    public string Name { get; }
    public string Description { get; }
    public PolicyResourceType ResourceType { get; }
    public PolicyEffect Effect { get; }
    public IReadOnlyList<string> Principals { get; }
    public IReadOnlyList<string> Actions { get; }
    public IReadOnlyList<string> Conditions { get; }
    public bool IsEnabled { get; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset? ExpiresAtUtc { get; }

    public bool IsExpired => ExpiresAtUtc.HasValue && ExpiresAtUtc.Value < DateTimeOffset.UtcNow;
}

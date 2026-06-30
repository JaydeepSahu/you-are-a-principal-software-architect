using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Domain;

public sealed class Tenant : AggregateRoot<TenantId>
{
    private Tenant()
        : base(TenantId.From(Guid.NewGuid()))
    {
        Name = string.Empty;
    }

    private Tenant(
        TenantId id,
        string name,
        Guid entraTenantId,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Name = name;
        EntraTenantId = entraTenantId;
        Status = TenantStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Name { get; private set; }

    public Guid EntraTenantId { get; private set; }

    public TenantStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Tenant Create(string name, Guid entraTenantId, DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name is required.", nameof(name));
        }

        if (entraTenantId == Guid.Empty)
        {
            throw new ArgumentException("Microsoft Entra tenant identifier is required.", nameof(entraTenantId));
        }

        return new Tenant(TenantId.New(), name.Trim(), entraTenantId, createdAtUtc);
    }

    public bool IsActive()
    {
        return Status == TenantStatus.Active;
    }

    public void Suspend(DateTimeOffset updatedAtUtc)
    {
        Status = TenantStatus.Suspended;
        UpdatedAtUtc = updatedAtUtc;
    }
}

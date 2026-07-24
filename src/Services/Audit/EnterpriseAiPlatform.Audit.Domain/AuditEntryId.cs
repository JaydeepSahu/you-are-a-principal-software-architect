namespace EnterpriseAiPlatform.Audit.Domain;

public sealed record AuditEntryId
{
    private AuditEntryId(Guid value) => Value = value;

    public Guid Value { get; }

    public static AuditEntryId New() => new(Guid.NewGuid());

    public static AuditEntryId From(Guid value)
        => value == Guid.Empty
            ? throw new ArgumentException("Audit entry identifier cannot be empty.", nameof(value))
            : new(value);

    public override string ToString() => Value.ToString("D");
}

namespace EnterpriseAiPlatform.SharedKernel;

public readonly record struct TenantId
{
    private TenantId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static TenantId New()
    {
        return new TenantId(Guid.NewGuid());
    }

    public static TenantId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Tenant identifier cannot be empty.", nameof(value));
        }

        return new TenantId(value);
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}

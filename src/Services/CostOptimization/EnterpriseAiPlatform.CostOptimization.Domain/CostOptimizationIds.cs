using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Domain;

public sealed record BudgetId(Guid Value) : StronglyTypedId(Value)
{
    public static BudgetId Create() => new(Guid.CreateVersion7());
    public static BudgetId Create(Guid value) => new(value);
    public override Guid CreateNew() => Guid.CreateVersion7();
    public override Guid CreateFrom(Guid value) => value;
}

public sealed record DepartmentQuotaId(Guid Value) : StronglyTypedId(Value)
{
    public static DepartmentQuotaId Create() => new(Guid.CreateVersion7());
    public static DepartmentQuotaId Create(Guid value) => new(value);
    public override Guid CreateNew() => Guid.CreateVersion7();
    public override Guid CreateFrom(Guid value) => value;
}

public sealed record UserQuotaId(Guid Value) : StronglyTypedId(Value)
{
    public static UserQuotaId Create() => new(Guid.CreateVersion7());
    public static UserQuotaId Create(Guid value) => new(value);
    public override Guid CreateNew() => Guid.CreateVersion7();
    public override Guid CreateFrom(Guid value) => value;
}

public sealed record ModelCostId(Guid Value) : StronglyTypedId(Value)
{
    public static ModelCostId Create() => new(Guid.CreateVersion7());
    public static ModelCostId Create(Guid value) => new(value);
    public override Guid CreateNew() => Guid.CreateVersion7();
    public override Guid CreateFrom(Guid value) => value;
}

public sealed record CostRecordId(Guid Value) : StronglyTypedId(Value)
{
    public static CostRecordId Create() => new(Guid.CreateVersion7());
    public static CostRecordId Create(Guid value) => new(value);
    public override Guid CreateNew() => Guid.CreateVersion7();
    public override Guid CreateFrom(Guid value) => value;
}

public sealed record AlertId(Guid Value) : StronglyTypedId(Value)
{
    public static AlertId Create() => new(Guid.CreateVersion7());
    public static AlertId Create(Guid value) => new(value);
    public override Guid CreateNew() => Guid.CreateVersion7();
    public override Guid CreateFrom(Guid value) => value;
}

public sealed record RoutingRuleId(Guid Value) : StronglyTypedId(Value)
{
    public static RoutingRuleId Create() => new(Guid.CreateVersion7());
    public static RoutingRuleId Create(Guid value) => new(value);
    public override Guid CreateNew() => Guid.CreateVersion7();
    public override Guid CreateFrom(Guid value) => value;
}

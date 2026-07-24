using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Domain;

public sealed class Budget : AggregateRoot<BudgetId>
{
    public TenantId TenantId { get; }
    public string Name { get; }
    public string? Description { get; }
    public CostAmount AllocatedAmount { get; private set; }
    public CostAmount SpentAmount { get; private set; }
    public BudgetPeriod Period { get; }
    public int PeriodYear { get; }
    public int PeriodMonth { get; }
    public int? PeriodQuarter { get; }
    public BudgetStatus Status { get; private set; }
    public string? DepartmentId { get; }
    public string? ProjectId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? SuspendedAt { get; }
    public string? SuspendedReason { get; }

    internal Budget(
        BudgetId id,
        TenantId tenantId,
        string name,
        string? description,
        CostAmount allocatedAmount,
        BudgetPeriod period,
        int periodYear,
        int periodMonth,
        int? periodQuarter,
        string? departmentId,
        string? projectId,
        DateTimeOffset createdAt)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        AllocatedAmount = allocatedAmount;
        SpentAmount = CostAmount.Zero(allocatedAmount.Currency);
        Period = period;
        PeriodYear = periodYear;
        PeriodMonth = periodMonth;
        PeriodQuarter = periodQuarter;
        Status = BudgetStatus.Active;
        DepartmentId = departmentId;
        ProjectId = projectId;
        CreatedAt = createdAt;
    }

    public static Budget Create(
        TenantId tenantId,
        string name,
        string? description,
        CostAmount allocatedAmount,
        BudgetPeriod period,
        int periodYear,
        int periodMonth,
        int? periodQuarter,
        string? departmentId,
        string? projectId)
    {
        return new Budget(
            BudgetId.Create(),
            tenantId,
            name,
            description,
            allocatedAmount,
            period,
            periodYear,
            periodMonth,
            periodQuarter,
            departmentId,
            projectId,
            DateTimeOffset.UtcNow);
    }

    public void AddSpending(CostAmount amount)
    {
        if (Status == BudgetStatus.Suspended)
            throw new InvalidOperationException("Cannot add spending to a suspended budget");

        SpentAmount = SpentAmount.Add(amount);
        UpdateStatus();
    }

    public void RemoveSpending(CostAmount amount)
    {
        SpentAmount = SpentAmount.Subtract(amount);
        if (SpentAmount.IsNegative)
            SpentAmount = CostAmount.Zero(SpentAmount.Currency);
        UpdateStatus();
    }

    public void UpdateAllocation(CostAmount newAmount)
    {
        if (newAmount.Currency != AllocatedAmount.Currency)
            throw new InvalidOperationException("Cannot change currency of a budget");
        AllocatedAmount = newAmount;
        UpdateStatus();
    }

    public void Suspend(string reason)
    {
        if (Status == BudgetStatus.Suspended)
            return;
        Status = BudgetStatus.Suspended;
    }

    public void Resume()
    {
        if (Status != BudgetStatus.Suspended)
            return;
        Status = BudgetStatus.Active;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var percentUsed = SpentAmount.AsPercentageOf(AllocatedAmount);
        Status = percentUsed switch
        {
            >= 100 => BudgetStatus.Exceeded,
            >= 80 => BudgetStatus.NearLimit,
            _ => BudgetStatus.Active
        };
    }

    public CostAmount RemainingAmount => AllocatedAmount.Subtract(SpentAmount);
    public decimal UtilizationPercentage => SpentAmount.AsPercentageOf(AllocatedAmount);
    public bool IsExceeded => Status == BudgetStatus.Exceeded;
    public bool IsNearLimit => Status == BudgetStatus.NearLimit;
    public bool IsActive => Status == BudgetStatus.Active;
}

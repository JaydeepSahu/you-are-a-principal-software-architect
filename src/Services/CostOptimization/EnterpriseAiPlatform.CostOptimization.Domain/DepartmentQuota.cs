using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Domain;

public sealed class DepartmentQuota : Entity<DepartmentQuotaId>
{
    public TenantId TenantId { get; }
    public string DepartmentId { get; }
    public string DepartmentName { get; }
    public CostAmount MonthlyLimit { get; private set; }
    public CostAmount MonthlyUsage { get; private set; }
    public int MaxUsers { get; private set; }
    public int ActiveUsers { get; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; }

    internal DepartmentQuota(
        DepartmentQuotaId id,
        TenantId tenantId,
        string departmentId,
        string departmentName,
        CostAmount monthlyLimit,
        int maxUsers,
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt)
        : base(id)
    {
        TenantId = tenantId;
        DepartmentId = departmentId;
        DepartmentName = departmentName;
        MonthlyLimit = monthlyLimit;
        MonthlyUsage = CostAmount.Zero(monthlyLimit.Currency);
        MaxUsers = maxUsers;
        ActiveUsers = 0;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public static DepartmentQuota Create(
        TenantId tenantId,
        string departmentId,
        string departmentName,
        CostAmount monthlyLimit,
        int maxUsers,
        DateTimeOffset? expiresAt)
    {
        return new DepartmentQuota(
            DepartmentQuotaId.Create(),
            tenantId,
            departmentId,
            departmentName,
            monthlyLimit,
            maxUsers,
            DateTimeOffset.UtcNow,
            expiresAt);
    }

    public void AddUsage(CostAmount amount)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot add usage to an inactive quota");
        MonthlyUsage = MonthlyUsage.Add(amount);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLimit(CostAmount newLimit)
    {
        if (newLimit.Currency != MonthlyLimit.Currency)
            throw new InvalidOperationException("Cannot change currency of a quota");
        MonthlyLimit = newLimit;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMaxUsers(int maxUsers)
    {
        if (maxUsers < ActiveUsers)
            throw new InvalidOperationException($"Cannot set max users below current active users ({ActiveUsers})");
        MaxUsers = maxUsers;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetUsage()
    {
        MonthlyUsage = CostAmount.Zero(MonthlyLimit.Currency);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public CostAmount RemainingAmount => MonthlyLimit.Subtract(MonthlyUsage);
    public decimal UtilizationPercentage => MonthlyUsage.AsPercentageOf(MonthlyLimit);
    public bool IsExceeded => MonthlyUsage.Value >= MonthlyLimit.Value;
    public bool HasCapacity => ActiveUsers < MaxUsers;
}

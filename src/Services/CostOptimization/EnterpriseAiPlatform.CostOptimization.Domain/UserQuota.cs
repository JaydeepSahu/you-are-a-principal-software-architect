using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Domain;

public sealed class UserQuota : Entity<UserQuotaId>
{
    public TenantId TenantId { get; }
    public string UserId { get; }
    public string? DepartmentId { get; }
    public CostAmount MonthlyLimit { get; private set; }
    public CostAmount MonthlyUsage { get; private set; }
    public int MaxRequestsPerDay { get; private set; }
    public int RequestsToday { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; }
    public DateTimeOffset LastResetAt { get; private set; }

    internal UserQuota(
        UserQuotaId id,
        TenantId tenantId,
        string userId,
        string? departmentId,
        CostAmount monthlyLimit,
        int maxRequestsPerDay,
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        DepartmentId = departmentId;
        MonthlyLimit = monthlyLimit;
        MonthlyUsage = CostAmount.Zero(monthlyLimit.Currency);
        MaxRequestsPerDay = maxRequestsPerDay;
        RequestsToday = 0;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        ExpiresAt = expiresAt;
        LastResetAt = createdAt;
    }

    public static UserQuota Create(
        TenantId tenantId,
        string userId,
        string? departmentId,
        CostAmount monthlyLimit,
        int maxRequestsPerDay,
        DateTimeOffset? expiresAt)
    {
        return new UserQuota(
            UserQuotaId.Create(),
            tenantId,
            userId,
            departmentId,
            monthlyLimit,
            maxRequestsPerDay,
            DateTimeOffset.UtcNow,
            expiresAt);
    }

    public void AddUsage(CostAmount amount)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot add usage to an inactive quota");
        if (IsExpired())
            throw new InvalidOperationException("Cannot add usage to an expired quota");
        
        MonthlyUsage = MonthlyUsage.Add(amount);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementRequests()
    {
        if (!CanMakeRequest())
            throw new InvalidOperationException("User has exceeded daily request limit");
        RequestsToday++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLimit(CostAmount newLimit)
    {
        if (newLimit.Currency != MonthlyLimit.Currency)
            throw new InvalidOperationException("Cannot change currency of a quota");
        MonthlyLimit = newLimit;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMaxRequestsPerDay(int maxRequests)
    {
        MaxRequestsPerDay = maxRequests;
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

    public void ResetDailyRequests()
    {
        RequestsToday = 0;
        LastResetAt = DateTimeOffset.UtcNow;
    }

    public void ResetMonthlyUsage()
    {
        MonthlyUsage = CostAmount.Zero(MonthlyLimit.Currency);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private bool IsExpired() => ExpiresAt.HasValue && DateTimeOffset.UtcNow > ExpiresAt.Value;

    public CostAmount RemainingAmount => MonthlyLimit.Subtract(MonthlyUsage);
    public decimal UtilizationPercentage => MonthlyUsage.AsPercentageOf(MonthlyLimit);
    public bool IsExceeded => MonthlyUsage.Value >= MonthlyLimit.Value;
    public bool CanMakeRequest() => RequestsToday < MaxRequestsPerDay && !IsExpired();
    public int RemainingRequests => Math.Max(0, MaxRequestsPerDay - RequestsToday);
}

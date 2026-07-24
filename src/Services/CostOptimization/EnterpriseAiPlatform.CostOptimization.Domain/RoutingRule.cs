using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Domain;

public sealed class RoutingRule : AggregateRoot<RoutingRuleId>
{
    public TenantId TenantId { get; }
    public string Name { get; }
    public string Description { get; }
    public string TriggerType { get; }
    public string SourceModelId { get; }
    public string TargetModelId { get; }
    public decimal CostThreshold { get; }
    public decimal UsageThresholdPercent { get; }
    public bool IsEnabled { get; private set; }
    public bool IsAutomatic { get; }
    public int Priority { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? LastTriggeredAt { get; private set; }
    public int TriggerCount { get; private set; }
    public DateTimeOffset? ExpiresAt { get; }
    public Dictionary<string, string> Conditions { get; }

    internal RoutingRule(
        RoutingRuleId id,
        TenantId tenantId,
        string name,
        string description,
        string triggerType,
        string sourceModelId,
        string targetModelId,
        decimal costThreshold,
        decimal usageThresholdPercent,
        bool isAutomatic,
        int priority,
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt,
        Dictionary<string, string> conditions)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        TriggerType = triggerType;
        SourceModelId = sourceModelId;
        TargetModelId = targetModelId;
        CostThreshold = costThreshold;
        UsageThresholdPercent = usageThresholdPercent;
        IsEnabled = true;
        IsAutomatic = isAutomatic;
        Priority = priority;
        CreatedAt = createdAt;
        LastTriggeredAt = null;
        TriggerCount = 0;
        ExpiresAt = expiresAt;
        Conditions = conditions;
    }

    public static RoutingRule Create(
        TenantId tenantId,
        string name,
        string description,
        string triggerType,
        string sourceModelId,
        string targetModelId,
        decimal costThreshold,
        decimal usageThresholdPercent,
        bool isAutomatic,
        int priority,
        DateTimeOffset? expiresAt,
        Dictionary<string, string>? conditions = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(sourceModelId))
            throw new ArgumentException("Source model cannot be empty", nameof(sourceModelId));
        if (string.IsNullOrWhiteSpace(targetModelId))
            throw new ArgumentException("Target model cannot be empty", nameof(targetModelId));

        return new RoutingRule(
            RoutingRuleId.Create(),
            tenantId,
            name,
            description,
            triggerType,
            sourceModelId,
            targetModelId,
            costThreshold,
            usageThresholdPercent,
            isAutomatic,
            priority,
            DateTimeOffset.UtcNow,
            expiresAt,
            conditions ?? new Dictionary<string, string>());
    }

    public bool ShouldTrigger(decimal currentCost, decimal usagePercent)
    {
        if (!IsEnabled)
            return false;
        if (IsExpired())
            return false;

        return currentCost >= CostThreshold || usagePercent >= UsageThresholdPercent;
    }

    public void RecordTrigger()
    {
        LastTriggeredAt = DateTimeOffset.UtcNow;
        TriggerCount++;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public bool IsExpired() => ExpiresAt.HasValue && DateTimeOffset.UtcNow > ExpiresAt.Value;
}

public sealed class CostForecast : ValueObject
{
    public int Year { get; }
    public int Month { get; }
    public decimal ProjectedTotalCost { get; }
    public decimal ProjectedOverage { get; }
    public decimal DailyAverageSoFar { get; }
    public decimal ProjectedDailyAverage { get; }
    public decimal TrendPercentage { get; }
    public List<DailyCostProjection> DailyProjections { get; }
    public DateTimeOffset GeneratedAt { get; }
    public DateTimeOffset BasedOnDataThrough { get; }

    public CostForecast(
        int year,
        int month,
        decimal projectedTotalCost,
        decimal projectedOverage,
        decimal dailyAverageSoFar,
        decimal projectedDailyAverage,
        decimal trendPercentage,
        List<DailyCostProjection> dailyProjections,
        DateTimeOffset generatedAt,
        DateTimeOffset basedOnDataThrough)
    {
        Year = year;
        Month = month;
        ProjectedTotalCost = projectedTotalCost;
        ProjectedOverage = projectedOverage;
        DailyAverageSoFar = dailyAverageSoFar;
        ProjectedDailyAverage = projectedDailyAverage;
        TrendPercentage = trendPercentage;
        DailyProjections = dailyProjections;
        GeneratedAt = generatedAt;
        BasedOnDataThrough = basedOnDataThrough;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Year;
        yield return Month;
        yield return ProjectedTotalCost;
        yield return GeneratedAt;
    }
}

public sealed record DailyCostProjection(int Day, decimal ProjectedCost, decimal LowerBound, decimal UpperBound);

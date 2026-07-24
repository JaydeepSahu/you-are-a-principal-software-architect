namespace EnterpriseAiPlatform.CostOptimization.Contracts.Responses;

public sealed record AlertResponse(
    string Id,
    string Type,
    string Title,
    string Message,
    string Severity,
    string Status,
    string? DepartmentId,
    string? UserId,
    string? BudgetId,
    decimal ThresholdValue,
    decimal CurrentValue,
    decimal PercentageUsed,
    DateTimeOffset TriggeredAt,
    DateTimeOffset? AcknowledgedAt,
    string? AcknowledgedBy,
    DateTimeOffset? ResolvedAt,
    string? ResolvedBy,
    string? ResolutionNotes);

public sealed record RoutingRuleResponse(
    string Id,
    string Name,
    string Description,
    string TriggerType,
    string SourceModelId,
    string TargetModelId,
    decimal CostThreshold,
    decimal UsageThresholdPercent,
    bool IsEnabled,
    bool IsAutomatic,
    int Priority,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastTriggeredAt,
    int TriggerCount,
    DateTimeOffset? ExpiresAt);

public sealed record RoutingRuleTriggerResponse(
    string RuleId,
    string RuleName,
    string SourceModelId,
    string TargetModelId,
    bool WasTriggered,
    DateTimeOffset? TriggeredAt,
    string? Message);

public sealed record CostForecastResponse(
    int Year,
    int Month,
    string? DepartmentId,
    decimal ProjectedTotalCost,
    decimal ProjectedOverage,
    decimal DailyAverageSoFar,
    decimal ProjectedDailyAverage,
    decimal TrendPercentage,
    List<DailyProjectionResponse> DailyProjections,
    DateTimeOffset GeneratedAt,
    DateTimeOffset BasedOnDataThrough,
    List<string> Recommendations);

public sealed record DailyProjectionResponse(
    int Day,
    decimal ProjectedCost,
    decimal LowerBound,
    decimal UpperBound);

public sealed record MonthlyReportResponse(
    int Year,
    int Month,
    string? DepartmentId,
    string ReportType,
    DateTimeOffset GeneratedAt,
    PeriodSummary Period,
    BudgetSummary Budget,
    UsageSummary Usage,
    TopConsumersSummary TopConsumers,
    CostTrendSummary Trends,
    RecommendationsSummary Recommendations);

public sealed record PeriodSummary(
    decimal TotalCost,
    string Currency,
    int TotalRequests,
    int TotalInputTokens,
    int TotalOutputTokens,
    decimal AverageCostPerRequest,
    decimal AverageCostPerToken,
    decimal VersusLastMonthPercent,
    decimal VersusBudgetPercent);

public sealed record BudgetSummary(
    decimal Allocated,
    decimal Spent,
    decimal Remaining,
    decimal UtilizationPercent,
    string Status,
    decimal ProjectedEndOfMonth,
    decimal ProjectedOverage);

public sealed record UsageSummary(
    Dictionary<string, int> RequestsByProvider,
    Dictionary<string, int> RequestsByModel,
    Dictionary<string, int> RequestsByCategory,
    Dictionary<string, decimal> CostByProvider,
    Dictionary<string, decimal> CostByModel,
    Dictionary<string, decimal> CostByCategory);

public sealed record TopConsumersSummary(
    List<ConsumerInfo> TopUsers,
    List<ConsumerInfo> TopDepartments,
    List<ConsumerInfo> TopModels);

public sealed record ConsumerInfo(
    string Id,
    string Name,
    decimal Cost,
    int Requests,
    decimal PercentOfTotal);

public sealed record CostTrendSummary(
    List<DailyTrend> Daily,
    List<WeeklyTrend> Weekly,
    decimal MonthToDateGrowth,
    decimal ProjectedMonthEndGrowth);

public sealed record DailyTrend(
    int Day,
    decimal Cost,
    decimal CumulativeCost,
    decimal VersusAverage);

public sealed record WeeklyTrend(
    int Week,
    decimal Cost,
    decimal VersusPreviousWeek);

public sealed record RecommendationsSummary(
    List<string> CostSavingTips,
    List<string> OptimizationOpportunities,
    List<string> RiskWarnings);

public sealed record CostCalculationResponse(
    string ProviderId,
    string ModelId,
    int InputTokens,
    int OutputTokens,
    double? ProcessingSeconds,
    decimal EstimatedCost,
    string Currency,
    decimal InputCost,
    decimal OutputCost,
    decimal RequestCost,
    decimal ComputeCost);

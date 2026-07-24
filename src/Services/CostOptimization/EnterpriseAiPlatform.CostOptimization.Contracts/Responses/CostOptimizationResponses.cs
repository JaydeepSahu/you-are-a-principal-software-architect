namespace EnterpriseAiPlatform.CostOptimization.Contracts.Responses;

public sealed record BudgetResponse(
    string Id,
    string Name,
    string? Description,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    decimal UtilizationPercent,
    string Currency,
    string Period,
    int PeriodYear,
    int PeriodMonth,
    int? PeriodQuarter,
    string Status,
    string? DepartmentId,
    string? ProjectId,
    DateTimeOffset CreatedAt);

public sealed record DepartmentQuotaResponse(
    string Id,
    string DepartmentId,
    string DepartmentName,
    decimal MonthlyLimit,
    decimal MonthlyUsage,
    decimal RemainingAmount,
    decimal UtilizationPercent,
    string Currency,
    int MaxUsers,
    int ActiveUsers,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ExpiresAt);

public sealed record UserQuotaResponse(
    string Id,
    string UserId,
    string? DepartmentId,
    decimal MonthlyLimit,
    decimal MonthlyUsage,
    decimal RemainingAmount,
    decimal UtilizationPercent,
    string Currency,
    int MaxRequestsPerDay,
    int RequestsToday,
    int RemainingRequests,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ExpiresAt);

public sealed record ModelCostResponse(
    string Id,
    string ProviderId,
    string ModelId,
    string ModelName,
    decimal InputCostPerToken,
    decimal OutputCostPerToken,
    decimal CostPerRequest,
    decimal CostPerSecond,
    string Currency,
    bool IsActive,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset CreatedAt);

public sealed record CostRecordResponse(
    string Id,
    string UserId,
    string? DepartmentId,
    string ProviderId,
    string ModelId,
    string Category,
    decimal Amount,
    string Currency,
    int InputTokens,
    int OutputTokens,
    int TotalTokens,
    double? ProcessingSeconds,
    string RequestId,
    string? CorrelationId,
    DateTimeOffset RecordedAt,
    int PeriodYear,
    int PeriodMonth,
    int? PeriodDay);

public sealed record CostSummaryResponse(
    int Year,
    int Month,
    string? DepartmentId,
    string? UserId,
    decimal TotalCost,
    string Currency,
    int TotalRequests,
    int TotalInputTokens,
    int TotalOutputTokens,
    Dictionary<string, decimal> CostByProvider,
    Dictionary<string, decimal> CostByModel,
    Dictionary<string, decimal> CostByCategory,
    List<DailyCostSummary> DailyBreakdown,
    decimal MonthToDateBudgetUtilization,
    decimal ProjectedMonthEndCost,
    decimal BudgetOverage);

public sealed record DailyCostSummary(
    int Day,
    decimal Cost,
    int Requests,
    int Tokens);

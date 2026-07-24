namespace EnterpriseAiPlatform.CostOptimization.Domain.Entities;

public sealed record DepartmentChargeback(
    string DepartmentId,
    string DepartmentName,
    string CostCenter,
    long ConsumedTokens,
    decimal AllocatedCostDollars,
    double PercentageOfTotalBudget);

public sealed record CostForecastResult(
    string TenantId,
    decimal CurrentMonthToDateSpendDollars,
    decimal DailyRunRateDollars,
    decimal ProjectedEndOfMonthSpendDollars,
    decimal MonthlyBudgetLimitDollars,
    double ProjectedBudgetPercentage,
    string ForecastStatus); // "OnTrack", "Warning", "Exceeded"

public sealed record TenantBudgetStatus(
    string TenantId,
    decimal MonthlyBudgetLimitDollars,
    decimal ConsumedDollars,
    double UsagePercentage,
    bool SoftWarningTriggered,
    bool HardQuotaBlockTriggered,
    List<DepartmentChargeback> DepartmentBreakdown);

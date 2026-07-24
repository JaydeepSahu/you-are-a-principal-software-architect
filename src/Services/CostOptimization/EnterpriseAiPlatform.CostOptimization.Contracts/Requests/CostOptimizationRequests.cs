namespace EnterpriseAiPlatform.CostOptimization.Contracts.Requests;

public sealed record CreateBudgetRequest(
    string Name,
    string? Description,
    decimal AllocatedAmount,
    string Currency,
    string Period,
    int PeriodYear,
    int PeriodMonth,
    int? PeriodQuarter,
    string? DepartmentId,
    string? ProjectId);

public sealed record UpdateBudgetRequest(
    decimal? AllocatedAmount,
    string? Status);

public sealed record AddSpendingRequest(
    decimal Amount,
    string Currency,
    string Category,
    int InputTokens,
    int OutputTokens,
    string ProviderId,
    string ModelId,
    string RequestId,
    string? CorrelationId,
    string? DepartmentId);

public sealed record QueryBudgetsRequest(
    string? DepartmentId,
    string? ProjectId,
    string? Status,
    int? PeriodYear,
    int? PeriodMonth,
    int Skip,
    int Take);

public sealed record CreateDepartmentQuotaRequest(
    string DepartmentId,
    string DepartmentName,
    decimal MonthlyLimit,
    string Currency,
    int MaxUsers,
    DateTimeOffset? ExpiresAt);

public sealed record UpdateDepartmentQuotaRequest(
    decimal? MonthlyLimit,
    int? MaxUsers,
    bool? IsActive);

public sealed record CreateUserQuotaRequest(
    string UserId,
    string? DepartmentId,
    decimal MonthlyLimit,
    string Currency,
    int MaxRequestsPerDay,
    DateTimeOffset? ExpiresAt);

public sealed record UpdateUserQuotaRequest(
    decimal? MonthlyLimit,
    int? MaxRequestsPerDay,
    bool? IsActive);

public sealed record QueryQuotasRequest(
    string? DepartmentId,
    string? UserId,
    bool? IsActive,
    int Skip,
    int Take);

using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.CostOptimization.Contracts.Responses;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationQueries;

public sealed record GetBudgetByIdQuery(
    Guid BudgetId,
    Guid CorrelationId) : IRequest<Result<BudgetResponse>>;

public sealed record GetBudgetsQuery(
    QueryBudgetsRequest Request,
    Guid CorrelationId) : IRequest<Result<PaginatedResult<BudgetResponse>>>;

public sealed record GetDepartmentQuotaByIdQuery(
    Guid QuotaId,
    Guid CorrelationId) : IRequest<Result<DepartmentQuotaResponse>>;

public sealed record GetDepartmentQuotaByDepartmentIdQuery(
    string DepartmentId,
    Guid CorrelationId) : IRequest<Result<DepartmentQuotaResponse>>;

public sealed record GetDepartmentQuotasQuery(
    QueryQuotasRequest Request,
    Guid CorrelationId) : IRequest<Result<PaginatedResult<DepartmentQuotaResponse>>>;

public sealed record GetUserQuotaByIdQuery(
    Guid QuotaId,
    Guid CorrelationId) : IRequest<Result<UserQuotaResponse>>;

public sealed record GetUserQuotaByUserIdQuery(
    string UserId,
    Guid CorrelationId) : IRequest<Result<UserQuotaResponse>>;

public sealed record GetUserQuotasQuery(
    QueryQuotasRequest Request,
    Guid CorrelationId) : IRequest<Result<PaginatedResult<UserQuotaResponse>>>;

public sealed record GetModelCostByIdQuery(
    Guid ModelCostId,
    Guid CorrelationId) : IRequest<Result<ModelCostResponse>>;

public sealed record GetModelCostByProviderModelQuery(
    string ProviderId,
    string ModelId,
    Guid CorrelationId) : IRequest<Result<ModelCostResponse>>;

public sealed record GetModelCostsQuery(
    QueryModelCostsRequest Request,
    Guid CorrelationId) : IRequest<Result<PaginatedResult<ModelCostResponse>>>;

public sealed record CalculateCostQuery(
    CalculateCostRequest Request,
    Guid CorrelationId) : IRequest<Result<CostCalculationResponse>>;

public sealed record GetCostRecordsQuery(
    QueryCostRecordsRequest Request,
    Guid CorrelationId) : IRequest<Result<PaginatedResult<CostRecordResponse>>>;

public sealed record GetCostSummaryQuery(
    GetCostSummaryRequest Request,
    Guid CorrelationId) : IRequest<Result<CostSummaryResponse>>;

public sealed record GetAlertByIdQuery(
    Guid AlertId,
    Guid CorrelationId) : IRequest<Result<AlertResponse>>;

public sealed record GetAlertsQuery(
    QueryAlertsRequest Request,
    Guid CorrelationId) : IRequest<Result<PaginatedResult<AlertResponse>>>;

public sealed record GetActiveAlertsQuery(
    Guid CorrelationId) : IRequest<Result<IReadOnlyList<AlertResponse>>>;

public sealed record GetRoutingRuleByIdQuery(
    Guid RuleId,
    Guid CorrelationId) : IRequest<Result<RoutingRuleResponse>>;

public sealed record GetRoutingRulesQuery(
    QueryRoutingRulesRequest Request,
    Guid CorrelationId) : IRequest<Result<PaginatedResult<RoutingRuleResponse>>>;

public sealed record GenerateForecastQuery(
    GenerateForecastRequest Request,
    Guid CorrelationId) : IRequest<Result<CostForecastResponse>>;

public sealed record GenerateMonthlyReportQuery(
    GenerateReportRequest Request,
    Guid CorrelationId) : IRequest<Result<MonthlyReportResponse>>;

public sealed record GetCostTrendQuery(
    int Year,
    int Month,
    string? DepartmentId,
    Guid CorrelationId) : IRequest<Result<CostTrendResponse>>;

public sealed record GetTopConsumersQuery(
    int Year,
    int Month,
    string? DepartmentId,
    int Top,
    Guid CorrelationId) : IRequest<Result<TopConsumersResponse>>;

public sealed record CostTrendResponse(
    int Year,
    int Month,
    List<DailyTrendItem> Daily,
    decimal MonthToDateGrowth,
    decimal ProjectedMonthEnd);

public sealed record DailyTrendItem(
    int Day,
    decimal Cost,
    decimal Cumulative,
    decimal VersusAverage);

public sealed record TopConsumersResponse(
    int Year,
    int Month,
    List<ConsumerItem> TopUsers,
    List<ConsumerItem> TopDepartments,
    List<ConsumerItem> TopModels);

public sealed record ConsumerItem(
    string Id,
    string Name,
    decimal Cost,
    int Requests,
    decimal PercentOfTotal);

public sealed record PaginatedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Skip,
    int Take)
{
    public int PageCount => (int)Math.Ceiling((double)TotalCount / Take);
    public bool HasNextPage => Skip + Take < TotalCount;
    public bool HasPreviousPage => Skip > 0;
}

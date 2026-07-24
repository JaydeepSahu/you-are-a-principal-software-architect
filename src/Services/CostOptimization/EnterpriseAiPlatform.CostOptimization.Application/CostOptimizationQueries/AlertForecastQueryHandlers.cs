using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationQueries;
using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.CostOptimization.Contracts.Responses;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationQueries;

internal sealed class GetAlertByIdHandler : IRequestHandler<GetAlertByIdQuery, Result<AlertResponse>>
{
    private readonly IAlertRepository _alertRepository;
    private readonly IRequestContext _requestContext;

    public GetAlertByIdHandler(IAlertRepository alertRepository, IRequestContext requestContext)
    {
        _alertRepository = alertRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<AlertResponse>> Handle(GetAlertByIdQuery request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetByIdAsync(AlertId.Create(request.AlertId), _requestContext.TenantId, cancellationToken);
        if (alert is null)
            return Result.Failure<AlertResponse>(CostOptimizationErrors.AlertNotFound);

        return alert.MapToResponse();
    }
}

internal sealed class GetAlertsHandler : IRequestHandler<GetAlertsQuery, Result<PaginatedResult<AlertResponse>>>
{
    private readonly IAlertRepository _alertRepository;
    private readonly IRequestContext _requestContext;

    public GetAlertsHandler(IAlertRepository alertRepository, IRequestContext requestContext)
    {
        _alertRepository = alertRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<PaginatedResult<AlertResponse>>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        AlertStatus? status = null;
        if (!string.IsNullOrEmpty(request.Request.Status) && Enum.TryParse<AlertStatus>(request.Request.Status, true, out var s))
            status = s;

        AlertSeverity? severity = null;
        if (!string.IsNullOrEmpty(request.Request.Severity) && Enum.TryParse<AlertSeverity>(request.Request.Severity, true, out var sev))
            severity = sev;

        var alerts = await _alertRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            request.Request.UserId,
            status,
            severity,
            request.Request.Type,
            request.Request.From,
            request.Request.To,
            request.Request.Skip,
            request.Request.Take,
            cancellationToken);

        var totalCount = await _alertRepository.CountAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            request.Request.UserId,
            status,
            severity,
            request.Request.Type,
            request.Request.From,
            request.Request.To,
            cancellationToken);

        var items = alerts.Select(a => a.MapToResponse()).ToList();
        return new PaginatedResult<AlertResponse>(items, totalCount, request.Request.Skip, request.Request.Take);
    }
}

internal sealed class GetActiveAlertsHandler : IRequestHandler<GetActiveAlertsQuery, Result<IReadOnlyList<AlertResponse>>>
{
    private readonly IAlertRepository _alertRepository;
    private readonly IRequestContext _requestContext;

    public GetActiveAlertsHandler(IAlertRepository alertRepository, IRequestContext requestContext)
    {
        _alertRepository = alertRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<IReadOnlyList<AlertResponse>>> Handle(GetActiveAlertsQuery request, CancellationToken cancellationToken)
    {
        var alerts = await _alertRepository.GetActiveAlertsAsync(_requestContext.TenantId, cancellationToken);
        var responses = alerts.Select(a => a.MapToResponse()).ToList();
        return Result.Success<IReadOnlyList<AlertResponse>>(responses);
    }
}

internal sealed class GetRoutingRuleByIdHandler : IRequestHandler<GetRoutingRuleByIdQuery, Result<RoutingRuleResponse>>
{
    private readonly IRoutingRuleRepository _routingRuleRepository;
    private readonly IRequestContext _requestContext;

    public GetRoutingRuleByIdHandler(IRoutingRuleRepository routingRuleRepository, IRequestContext requestContext)
    {
        _routingRuleRepository = routingRuleRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<RoutingRuleResponse>> Handle(GetRoutingRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _routingRuleRepository.GetByIdAsync(RoutingRuleId.Create(request.RuleId), _requestContext.TenantId, cancellationToken);
        if (rule is null)
            return Result.Failure<RoutingRuleResponse>(CostOptimizationErrors.RoutingRuleNotFound);

        return rule.MapToResponse();
    }
}

internal sealed class GetRoutingRulesHandler : IRequestHandler<GetRoutingRulesQuery, Result<PaginatedResult<RoutingRuleResponse>>>
{
    private readonly IRoutingRuleRepository _routingRuleRepository;
    private readonly IRequestContext _requestContext;

    public GetRoutingRulesHandler(IRoutingRuleRepository routingRuleRepository, IRequestContext requestContext)
    {
        _routingRuleRepository = routingRuleRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<PaginatedResult<RoutingRuleResponse>>> Handle(GetRoutingRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _routingRuleRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.SourceModelId,
            request.Request.TargetModelId,
            request.Request.TriggerType,
            request.Request.IsEnabled,
            request.Request.Skip,
            request.Request.Take,
            cancellationToken);

        var totalCount = await _routingRuleRepository.CountAsync(
            _requestContext.TenantId,
            request.Request.SourceModelId,
            request.Request.TargetModelId,
            request.Request.TriggerType,
            request.Request.IsEnabled,
            cancellationToken);

        var items = rules.Select(r => r.MapToResponse()).ToList();
        return new PaginatedResult<RoutingRuleResponse>(items, totalCount, request.Request.Skip, request.Request.Take);
    }
}

internal sealed class GenerateForecastHandler : IRequestHandler<GenerateForecastQuery, Result<CostForecastResponse>>
{
    private readonly ICostRecordRepository _costRecordRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRequestContext _requestContext;

    public GenerateForecastHandler(
        ICostRecordRepository costRecordRepository,
        IBudgetRepository budgetRepository,
        IRequestContext requestContext)
    {
        _costRecordRepository = costRecordRepository;
        _budgetRepository = budgetRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<CostForecastResponse>> Handle(GenerateForecastQuery request, CancellationToken cancellationToken)
    {
        var aggregation = await _costRecordRepository.GetAggregationAsync(
            _requestContext.TenantId,
            request.Request.Year,
            request.Request.Month,
            request.Request.DepartmentId,
            null,
            cancellationToken);

        var daysInMonth = DateTime.DaysInMonth(request.Request.Year, request.Request.Month);
        var today = request.Request.Year == DateTimeOffset.UtcNow.Year && request.Request.Month == DateTimeOffset.UtcNow.Month
            ? DateTimeOffset.UtcNow.Day
            : daysInMonth;

        var dailyAverageSoFar = today > 0 ? aggregation.TotalCost / today : 0;
        var projectedTotalCost = dailyAverageSoFar * daysInMonth;

        var budgets = await _budgetRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            null,
            null,
            request.Request.Year,
            request.Request.Month,
            0, 1,
            cancellationToken);

        var budget = budgets.Count > 0 ? budgets[0] : null;
        var projectedOverage = budget is not null
            ? Math.Max(0, projectedTotalCost - budget.AllocatedAmount.Value)
            : 0;

        var trendPercentage = today > 1 && aggregation.TotalCost > 0
            ? ((dailyAverageSoFar * daysInMonth - aggregation.TotalCost) / aggregation.TotalCost) * 100
            : 0;

        var recommendations = new List<string>();
        if (projectedOverage > 0)
            recommendations.Add($"Projected overage of {projectedOverage:C}. Consider implementing cost controls.");
        if (trendPercentage > 20)
            recommendations.Add($"Cost growth rate is {trendPercentage:F1}% above average. Review usage patterns.");
        if (aggregation.CostByProvider.Count > 1)
        {
            var topProvider = aggregation.CostByProvider.MaxBy(x => x.Value);
            recommendations.Add($"Consider optimizing {topProvider.Key} usage which accounts for {(aggregation.TotalCost > 0 ? (topProvider.Value / aggregation.TotalCost * 100) : 0):F1}% of costs.");
        }

        var dailyProjections = new List<DailyProjectionResponse>();
        var stdDev = CalculateStdDev(aggregation.DailyBreakdown.Select(d => d.Cost).ToList());
        for (int day = 1; day <= daysInMonth; day++)
        {
            var projectedCost = dailyAverageSoFar;
            var lowerBound = Math.Max(0, projectedCost - stdDev);
            var upperBound = projectedCost + stdDev;
            dailyProjections.Add(new DailyProjectionResponse(day, projectedCost, lowerBound, upperBound));
        }

        return new CostForecastResponse(
            request.Request.Year,
            request.Request.Month,
            request.Request.DepartmentId,
            projectedTotalCost,
            projectedOverage,
            dailyAverageSoFar,
            dailyAverageSoFar,
            trendPercentage,
            dailyProjections,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(-today),
            recommendations);
    }

    private static decimal CalculateStdDev(List<decimal> values)
    {
        if (values.Count <= 1) return 0;
        var avg = values.Average();
        var sumOfSquares = values.Sum(v => (v - avg) * (v - avg));
        return (decimal)Math.Sqrt((double)(sumOfSquares / (values.Count - 1)));
    }
}

internal sealed class GenerateMonthlyReportHandler : IRequestHandler<GenerateMonthlyReportQuery, Result<MonthlyReportResponse>>
{
    private readonly ICostRecordRepository _costRecordRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRequestContext _requestContext;

    public GenerateMonthlyReportHandler(
        ICostRecordRepository costRecordRepository,
        IBudgetRepository budgetRepository,
        IRequestContext requestContext)
    {
        _costRecordRepository = costRecordRepository;
        _budgetRepository = budgetRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<MonthlyReportResponse>> Handle(GenerateMonthlyReportQuery request, CancellationToken cancellationToken)
    {
        var currentAggregation = await _costRecordRepository.GetAggregationAsync(
            _requestContext.TenantId,
            request.Request.Year,
            request.Request.Month,
            request.Request.DepartmentId,
            request.Request.UserId,
            cancellationToken);

        var previousMonth = request.Request.Month == 1 ? 12 : request.Request.Month - 1;
        var previousYear = request.Request.Month == 1 ? request.Request.Year - 1 : request.Request.Year;
        var previousAggregation = await _costRecordRepository.GetAggregationAsync(
            _requestContext.TenantId,
            previousYear,
            previousMonth,
            request.Request.DepartmentId,
            request.Request.UserId,
            cancellationToken);

        var daysInMonth = DateTime.DaysInMonth(request.Request.Year, request.Request.Month);
        var today = request.Request.Year == DateTimeOffset.UtcNow.Year && request.Request.Month == DateTimeOffset.UtcNow.Month
            ? DateTimeOffset.UtcNow.Day
            : daysInMonth;
        var projectedEndOfMonth = today > 0 ? currentAggregation.TotalCost / today * daysInMonth : 0;

        var budgets = await _budgetRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            null,
            null,
            request.Request.Year,
            request.Request.Month,
            0, 1,
            cancellationToken);

        var budget = budgets.Count > 0 ? budgets[0] : null;
        var budgetSummary = new BudgetSummary(
            budget?.AllocatedAmount.Value ?? 0,
            currentAggregation.TotalCost,
            budget?.RemainingAmount.Value ?? 0,
            budget?.UtilizationPercentage ?? 0,
            budget?.Status.ToString() ?? "NoBudget",
            projectedEndOfMonth,
            Math.Max(0, projectedEndOfMonth - (budget?.AllocatedAmount.Value ?? 0)));

        var versusLastMonthPercent = previousAggregation.TotalCost > 0
            ? (currentAggregation.TotalCost - previousAggregation.TotalCost) / previousAggregation.TotalCost * 100
            : 0;

        var versusBudgetPercent = budget is not null && budget.AllocatedAmount.Value > 0
            ? (currentAggregation.TotalCost - budget.AllocatedAmount.Value) / budget.AllocatedAmount.Value * 100
            : 0;

        var periodSummary = new PeriodSummary(
            currentAggregation.TotalCost,
            "USD",
            currentAggregation.TotalRequests,
            currentAggregation.TotalInputTokens,
            currentAggregation.TotalOutputTokens,
            currentAggregation.TotalRequests > 0 ? currentAggregation.TotalCost / currentAggregation.TotalRequests : 0,
            currentAggregation.TotalInputTokens > 0 ? currentAggregation.TotalCost / currentAggregation.TotalInputTokens * 1000 : 0,
            versusLastMonthPercent,
            versusBudgetPercent);

        var usageSummary = new UsageSummary(
            currentAggregation.RequestsByProvider,
            currentAggregation.RequestsByModel,
            currentAggregation.RequestsByCategory,
            currentAggregation.CostByProvider,
            currentAggregation.CostByModel,
            currentAggregation.CostByCategory);

        var topUsers = currentAggregation.CostByModel.Take(5).Select((kvp, i) => new ConsumerInfo(
            $"user_{i}",
            kvp.Key,
            kvp.Value,
            currentAggregation.RequestsByModel.GetValueOrDefault(kvp.Key),
            currentAggregation.TotalCost > 0 ? kvp.Value / currentAggregation.TotalCost * 100 : 0)).ToList();

        var topConsumers = new TopConsumersSummary(topUsers, new List<ConsumerInfo>(), new List<ConsumerInfo>());

        var dailyTrends = currentAggregation.DailyBreakdown.Select(d => new DailyTrend(
            d.Day,
            d.Cost,
            currentAggregation.DailyBreakdown.Take(d.Day).Sum(x => x.Cost),
            d.Cost - (currentAggregation.DailyBreakdown.Count > 0 ? currentAggregation.DailyBreakdown.Average(x => x.Cost) : 0))).ToList();

        var trends = new CostTrendSummary(
            dailyTrends,
            new List<WeeklyTrend>(),
            versusLastMonthPercent,
            projectedEndOfMonth - currentAggregation.TotalCost);

        var recommendations = new RecommendationsSummary(
            new List<string> { "Enable semantic caching to reduce redundant API calls", "Consider using smaller models for simple tasks" },
            new List<string>(),
            currentAggregation.TotalCost > (budget?.AllocatedAmount.Value ?? 0) * 0.8m
                ? new List<string> { "Budget threshold warning - approaching limit" }
                : new List<string>());

        return new MonthlyReportResponse(
            request.Request.Year,
            request.Request.Month,
            request.Request.DepartmentId,
            request.Request.ReportType,
            DateTimeOffset.UtcNow,
            periodSummary,
            budgetSummary,
            usageSummary,
            topConsumers,
            trends,
            recommendations);
    }
}

internal static class AlertRoutingMappingExtensions
{
    public static AlertResponse MapToResponse(this Alert alert) => new(
        alert.Id.Value.ToString(),
        alert.Type,
        alert.Title,
        alert.Message,
        alert.Severity.ToString(),
        alert.Status.ToString(),
        alert.DepartmentId,
        alert.UserId,
        alert.BudgetId,
        alert.ThresholdValue,
        alert.CurrentValue,
        alert.PercentageUsed,
        alert.TriggeredAt,
        alert.AcknowledgedAt,
        alert.AcknowledgedBy,
        alert.ResolvedAt,
        alert.ResolvedBy,
        alert.ResolutionNotes);

    public static RoutingRuleResponse MapToResponse(this RoutingRule rule) => new(
        rule.Id.Value.ToString(),
        rule.Name,
        rule.Description,
        rule.TriggerType,
        rule.SourceModelId,
        rule.TargetModelId,
        rule.CostThreshold,
        rule.UsageThresholdPercent,
        rule.IsEnabled,
        rule.IsAutomatic,
        rule.Priority,
        rule.CreatedAt,
        rule.LastTriggeredAt,
        rule.TriggerCount,
        rule.ExpiresAt);
}

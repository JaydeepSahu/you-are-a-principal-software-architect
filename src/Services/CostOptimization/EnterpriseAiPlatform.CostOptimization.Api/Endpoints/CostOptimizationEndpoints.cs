using EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationCommands;
using EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationQueries;
using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.CostOptimization.Api.Endpoints;

public static class CostOptimizationEndpoints
{
    public static IEndpointRouteBuilder MapCostOptimizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/cost-optimization");

        // Budget endpoints
        group.MapPost("/budgets", CreateBudgetAsync);
        group.MapGet("/budgets", GetBudgetsAsync);
        group.MapGet("/budgets/{budgetId}", GetBudgetByIdAsync);
        group.MapPut("/budgets/{budgetId}", UpdateBudgetAsync);
        group.MapPost("/budgets/{budgetId}/suspend", SuspendBudgetAsync);
        group.MapPost("/budgets/{budgetId}/resume", ResumeBudgetAsync);

        // Quota endpoints
        group.MapPost("/quotas/departments", CreateDepartmentQuotaAsync);
        group.MapGet("/quotas/departments", GetDepartmentQuotasAsync);
        group.MapGet("/quotas/departments/{quotaId}", GetDepartmentQuotaByIdAsync);
        group.MapPut("/quotas/departments/{quotaId}", UpdateDepartmentQuotaAsync);

        group.MapPost("/quotas/users", CreateUserQuotaAsync);
        group.MapGet("/quotas/users", GetUserQuotasAsync);
        group.MapGet("/quotas/users/{quotaId}", GetUserQuotaByIdAsync);
        group.MapGet("/quotas/users/by-user/{userId}", GetUserQuotaByUserIdAsync);
        group.MapPut("/quotas/users/{quotaId}", UpdateUserQuotaAsync);

        // Cost record endpoints
        group.MapPost("/costs/record", RecordCostAsync);
        group.MapGet("/costs/records", GetCostRecordsAsync);
        group.MapGet("/costs/summary", GetCostSummaryAsync);
        group.MapPost("/costs/calculate", CalculateCostAsync);

        // Model cost endpoints
        group.MapPost("/model-costs", CreateModelCostAsync);
        group.MapGet("/model-costs", GetModelCostsAsync);
        group.MapGet("/model-costs/{modelCostId}", GetModelCostByIdAsync);
        group.MapGet("/model-costs/by-provider-model", GetModelCostByProviderModelAsync);
        group.MapPut("/model-costs/{modelCostId}", UpdateModelCostAsync);

        // Alert endpoints
        group.MapGet("/alerts", GetAlertsAsync);
        group.MapGet("/alerts/{alertId}", GetAlertByIdAsync);
        group.MapGet("/alerts/active", GetActiveAlertsAsync);
        group.MapPost("/alerts/{alertId}/acknowledge", AcknowledgeAlertAsync);
        group.MapPost("/alerts/{alertId}/resolve", ResolveAlertAsync);

        // Routing rule endpoints
        group.MapPost("/routing-rules", CreateRoutingRuleAsync);
        group.MapGet("/routing-rules", GetRoutingRulesAsync);
        group.MapGet("/routing-rules/{ruleId}", GetRoutingRuleByIdAsync);
        group.MapPut("/routing-rules/{ruleId}", UpdateRoutingRuleAsync);
        group.MapPost("/routing-rules/{ruleId}/enable", EnableRoutingRuleAsync);
        group.MapPost("/routing-rules/{ruleId}/disable", DisableRoutingRuleAsync);
        group.MapPost("/routing-rules/{ruleId}/trigger", TriggerRoutingRuleAsync);

        // Forecast and reports
        group.MapPost("/forecast", GenerateForecastAsync);
        group.MapPost("/reports/monthly", GenerateMonthlyReportAsync);

        return endpoints;
    }

    private static async Task<IResult> CreateBudgetAsync(
        [FromBody] CreateBudgetRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateBudgetCommand(request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/cost-optimization/budgets/{result.Value.BudgetId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetBudgetsAsync(
        [FromQuery] string? departmentId,
        [FromQuery] string? projectId,
        [FromQuery] string? status,
        [FromQuery] int? periodYear,
        [FromQuery] int? periodMonth,
        [FromQuery] int skip,
        [FromQuery] int take,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBudgetsQuery(
            new QueryBudgetsRequest(departmentId, projectId, status, periodYear, periodMonth, skip, take),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetBudgetByIdAsync(
        [FromRoute] Guid budgetId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBudgetByIdQuery(budgetId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateBudgetAsync(
        [FromRoute] Guid budgetId,
        [FromBody] UpdateBudgetRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateBudgetCommand(budgetId, request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> SuspendBudgetAsync(
        [FromRoute] Guid budgetId,
        [FromQuery] string reason,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SuspendBudgetCommand(budgetId, reason, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> ResumeBudgetAsync(
        [FromRoute] Guid budgetId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResumeBudgetCommand(budgetId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> CreateDepartmentQuotaAsync(
        [FromBody] CreateDepartmentQuotaRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateDepartmentQuotaCommand(request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/cost-optimization/quotas/departments/{result.Value.QuotaId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetDepartmentQuotasAsync(
        [FromQuery] string? departmentId,
        [FromQuery] bool? isActive,
        [FromQuery] int skip,
        [FromQuery] int take,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDepartmentQuotasQuery(
            new QueryQuotasRequest(null, departmentId, isActive, skip, take),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetDepartmentQuotaByIdAsync(
        [FromRoute] Guid quotaId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDepartmentQuotaByIdQuery(quotaId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateDepartmentQuotaAsync(
        [FromRoute] Guid quotaId,
        [FromBody] UpdateDepartmentQuotaRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateDepartmentQuotaCommand(quotaId, request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> CreateUserQuotaAsync(
        [FromBody] CreateUserQuotaRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateUserQuotaCommand(request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/cost-optimization/quotas/users/{result.Value.QuotaId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetUserQuotasAsync(
        [FromQuery] string? departmentId,
        [FromQuery] string? userId,
        [FromQuery] bool? isActive,
        [FromQuery] int skip,
        [FromQuery] int take,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserQuotasQuery(
            new QueryQuotasRequest(departmentId, userId, isActive, skip, take),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetUserQuotaByIdAsync(
        [FromRoute] Guid quotaId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserQuotaByIdQuery(quotaId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetUserQuotaByUserIdAsync(
        [FromRoute] string userId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserQuotaByUserIdQuery(userId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateUserQuotaAsync(
        [FromRoute] Guid quotaId,
        [FromBody] UpdateUserQuotaRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateUserQuotaCommand(quotaId, request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> RecordCostAsync(
        [FromBody] RecordCostRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RecordCostCommand(request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/cost-optimization/costs/records/{result.Value.RecordId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetCostRecordsAsync(
        [FromQuery] string? userId,
        [FromQuery] string? departmentId,
        [FromQuery] string? providerId,
        [FromQuery] string? modelId,
        [FromQuery] string? category,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int skip,
        [FromQuery] int take,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCostRecordsQuery(
            new QueryCostRecordsRequest(userId, departmentId, providerId, modelId, category, from, to, skip, take),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetCostSummaryAsync(
        [FromQuery] string? departmentId,
        [FromQuery] string? userId,
        [FromQuery] int year,
        [FromQuery] int month,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCostSummaryQuery(
            new GetCostSummaryRequest(departmentId, userId, year, month),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> CalculateCostAsync(
        [FromBody] CalculateCostRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CalculateCostQuery(request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> CreateModelCostAsync(
        [FromBody] CreateModelCostRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateModelCostCommand(request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/cost-optimization/model-costs/{result.Value.ModelCostId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetModelCostsAsync(
        [FromQuery] string? providerId,
        [FromQuery] string? modelId,
        [FromQuery] bool? isActive,
        [FromQuery] int skip,
        [FromQuery] int take,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModelCostsQuery(
            new QueryModelCostsRequest(providerId, modelId, isActive, skip, take),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetModelCostByIdAsync(
        [FromRoute] Guid modelCostId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModelCostByIdQuery(modelCostId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetModelCostByProviderModelAsync(
        [FromQuery] string providerId,
        [FromQuery] string modelId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModelCostByProviderModelQuery(providerId, modelId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateModelCostAsync(
        [FromRoute] Guid modelCostId,
        [FromBody] UpdateModelCostRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateModelCostCommand(modelCostId, request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetAlertsAsync(
        [FromQuery] string? departmentId,
        [FromQuery] string? userId,
        [FromQuery] string? status,
        [FromQuery] string? severity,
        [FromQuery] string? type,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int skip,
        [FromQuery] int take,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAlertsQuery(
            new QueryAlertsRequest(departmentId, userId, status, severity, type, from, to, skip, take),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetAlertByIdAsync(
        [FromRoute] Guid alertId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAlertByIdQuery(alertId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetActiveAlertsAsync(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetActiveAlertsQuery(GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> AcknowledgeAlertAsync(
        [FromRoute] Guid alertId,
        [FromQuery] string acknowledgedBy,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AcknowledgeAlertCommand(
            new AcknowledgeAlertRequest(alertId.ToString(), acknowledgedBy),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> ResolveAlertAsync(
        [FromRoute] Guid alertId,
        [FromQuery] string resolvedBy,
        [FromQuery] string? notes,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResolveAlertCommand(
            new ResolveAlertRequest(alertId.ToString(), resolvedBy, notes),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> CreateRoutingRuleAsync(
        [FromBody] CreateRoutingRuleRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRoutingRuleCommand(request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/cost-optimization/routing-rules/{result.Value.RuleId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetRoutingRulesAsync(
        [FromQuery] string? sourceModelId,
        [FromQuery] string? targetModelId,
        [FromQuery] string? triggerType,
        [FromQuery] bool? isEnabled,
        [FromQuery] int skip,
        [FromQuery] int take,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRoutingRulesQuery(
            new QueryRoutingRulesRequest(sourceModelId, targetModelId, triggerType, isEnabled, skip, take),
            GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetRoutingRuleByIdAsync(
        [FromRoute] Guid ruleId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRoutingRuleByIdQuery(ruleId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateRoutingRuleAsync(
        [FromRoute] Guid ruleId,
        [FromBody] UpdateRoutingRuleRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateRoutingRuleCommand(ruleId, request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> EnableRoutingRuleAsync(
        [FromRoute] Guid ruleId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new EnableRoutingRuleCommand(ruleId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> DisableRoutingRuleAsync(
        [FromRoute] Guid ruleId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DisableRoutingRuleCommand(ruleId, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> TriggerRoutingRuleAsync(
        [FromRoute] Guid ruleId,
        [FromBody] TriggerRoutingRuleRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new TriggerRoutingRuleCommand(request with { RuleId = ruleId.ToString() }, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GenerateForecastAsync(
        [FromBody] GenerateForecastRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GenerateForecastQuery(request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GenerateMonthlyReportAsync(
        [FromBody] GenerateReportRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GenerateMonthlyReportQuery(request, GetCorrelationId(httpContext)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static Guid GetCorrelationId(HttpContext httpContext)
    {
        var correlationId = httpContext.TraceIdentifier;
        return Guid.TryParse(correlationId, out var id) ? id : Guid.NewGuid();
    }

    private static IResult ToProblem(ErrorDetail error)
        => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest, title: error.Code);
}

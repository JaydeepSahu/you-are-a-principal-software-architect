using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using static EnterpriseAiPlatform.ServiceDefaults.ServiceDefaultsExtensions;

namespace EnterpriseAiPlatform.CostOptimization.Api.Endpoints;

public static class CostOptimizationEndpoints
{
    public static IEndpointRouteBuilder MapCostOptimizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/cost")
            .WithTags("Financial Predictability & FinOps")
            .WithOpenApi()
            .RequireAuthorization(EnterpriseAuthorizationPolicies.Developer);

        group.MapGet("/chargeback", async (
            ICostAllocationEngine finOpsEngine,
            IRequestContextAccessor requestContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = requestContext.Current.TenantId;
            var result = await finOpsEngine.GenerateChargebackReportAsync(tenantId, cancellationToken: cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetDepartmentalChargeback")
        .WithSummary("Generate departmental cost allocation and token chargeback reports.")
        .WithDescription("Returns a breakdown of AI spend by department, cost centre, and token volume for the current billing period.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/forecast", async (
            ICostAllocationEngine finOpsEngine,
            IRequestContextAccessor requestContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = requestContext.Current.TenantId;
            var result = await finOpsEngine.ForecastMonthlySpendAsync(tenantId, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetMonthlySpendForecast")
        .WithSummary("Forecast end-of-month financial spend based on daily run-rate trends.")
        .WithDescription("Calculates the projected spend for the current month using the observed daily run-rate and the number of days remaining. Returns the forecast status: OnTrack, Warning, or Exceeded.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/budgets", async (
            ICostAllocationEngine finOpsEngine,
            IRequestContextAccessor requestContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = requestContext.Current.TenantId;
            var result = await finOpsEngine.GetTenantBudgetStatusAsync(tenantId, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetTenantBudgetStatus")
        .WithSummary("Inspect tenant token budget quota status, soft warnings, and departmental breakdowns.")
        .WithDescription("Returns current spend vs budget, utilisation percentage, whether soft-warning (75%) or hard-quota-block (100%) thresholds are triggered, and a per-department breakdown.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}

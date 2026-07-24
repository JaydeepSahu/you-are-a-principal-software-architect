using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnterpriseAiPlatform.CostOptimization.Api.Endpoints;

public static class CostOptimizationEndpoints
{
    public static IEndpointRouteBuilder MapCostOptimizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/cost")
            .WithTags("Financial Predictability & FinOps")
            .WithOpenApi();

        group.MapGet("/chargeback", async (
            ICostAllocationEngine finOpsEngine,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);
            var result = await finOpsEngine.GenerateChargebackReportAsync(tenantId, cancellationToken: cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetDepartmentalChargeback")
        .WithSummary("Generate departmental cost allocation and token chargeback reports.")
        .WithDescription("Returns a breakdown of AI spend by department, cost centre, and token volume for the current billing period. Requires the `X-Tenant-Id` header.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/forecast", async (
            ICostAllocationEngine finOpsEngine,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);
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
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);
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

    private static TenantId GetTenantId(HttpContext httpContext)
    {
        var tenantHeader = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        return tenantHeader is not null && Guid.TryParse(tenantHeader, out var tid)
            ? TenantId.From(tid)
            : TenantId.From(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    }
}

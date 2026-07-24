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
        .WithSummary("Generate departmental cost allocation and token chargeback reports.");

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
        .WithSummary("Forecast end-of-month financial spend based on daily run-rate trends.");

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
        .WithSummary("Inspect tenant token budget quota status, soft warnings, and departmental breakdowns.");

        return endpoints;
    }

    private static TenantId GetTenantId(HttpContext httpContext)
    {
        var tenantHeader = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        return TenantId.From(tenantHeader ?? "default-tenant");
    }
}

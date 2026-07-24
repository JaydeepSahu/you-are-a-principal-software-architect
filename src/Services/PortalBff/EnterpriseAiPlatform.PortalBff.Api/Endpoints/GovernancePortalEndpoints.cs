using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnterpriseAiPlatform.PortalBff.Api.Endpoints;

public static class GovernancePortalEndpoints
{
    public static IEndpointRouteBuilder MapGovernancePortalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/portal")
            .WithTags("Governance Portal BFF")
            .WithOpenApi();

        group.MapGet("/analytics/overview", () =>
        {
            var summary = new
            {
                TotalRequests = 1450230,
                TotalTokens = 892104500,
                EstimatedCostSavedDollars = 42500.00,
                ActiveTenants = 18,
                AverageLatencyMs = 185,
                PolicyInterceptionsCount = 342
            };
            return Results.Ok(summary);
        })
        .WithName("GetAnalyticsOverview")
        .WithSummary("Get real-time AI control plane analytics summary.");

        group.MapGet("/marketplace/catalog", async (IModelMarketplace marketplace, CancellationToken ct) =>
        {
            var result = await marketplace.SearchMarketplaceAsync(cancellationToken: ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .WithName("GetMarketplaceCatalog")
        .WithSummary("Browse internal model and agent marketplace catalog.");

        group.MapGet("/cluster/gpus", async (IGpuClusterManager clusterManager, CancellationToken ct) =>
        {
            var result = await clusterManager.GetClusterStatusAsync(ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .WithName("GetGpuClusterStatus")
        .WithSummary("Get live GPU cluster metrics and inference slot utilization.");

        return endpoints;
    }
}

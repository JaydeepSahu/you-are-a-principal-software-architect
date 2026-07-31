using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using static EnterpriseAiPlatform.ServiceDefaults.ServiceDefaultsExtensions;

namespace EnterpriseAiPlatform.PortalBff.Api.Endpoints;

public static class GovernancePortalEndpoints
{
    public static IEndpointRouteBuilder MapGovernancePortalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/portal")
            .WithTags("Governance Portal BFF")
            .WithOpenApi()
            .RequireAuthorization(EnterpriseAuthorizationPolicies.Auditor);

        group.MapGet("/analytics/overview", async (
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!PortalBackendProxy.TryGetBackendBaseUri(configuration, "Metering", out var meteringBaseUri))
            {
                return PortalBackendProxy.BackendNotConfigured("Metering");
            }

            return await PortalBackendProxy.ForwardJsonAsync(
                httpContext,
                httpClientFactory,
                meteringBaseUri,
                "/api/v1/metering/usage",
                cancellationToken);
        })
        .WithName("GetAnalyticsOverview")
        .WithSummary("Get AI control plane usage analytics from the Metering service.");

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

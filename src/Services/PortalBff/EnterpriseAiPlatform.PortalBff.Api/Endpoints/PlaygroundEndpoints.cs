using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using static EnterpriseAiPlatform.ServiceDefaults.ServiceDefaultsExtensions;

namespace EnterpriseAiPlatform.PortalBff.Api.Endpoints;

public sealed record ModelCompareRequest(
    string Prompt,
    List<string> TargetModels,
    double Temperature = 0.7);

public sealed record ModelCompareResult(
    string ModelId,
    string? ProviderName,
    string ResponseText,
    long LatencyMs,
    int? PromptTokens,
    int? CompletionTokens,
    decimal? EstimatedCostDollars,
    double? QualityScore);

public sealed record QuotaSummary(
    string TenantId,
    long MonthlyTokenLimit,
    long ConsumedTokens,
    decimal MonthlyBudgetDollars,
    decimal ConsumedDollars,
    string Status, // "Healthy", "Warning", "Exceeded"
    double UsagePercentage);

public static class PlaygroundEndpoints
{
    public static IEndpointRouteBuilder MapPlaygroundEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/portal/playground")
            .WithTags("Web Playground & Admin BFF")
            .WithOpenApi()
            .RequireAuthorization(EnterpriseAuthorizationPolicies.Developer);

        group.MapPost("/compare", async (
            ModelCompareRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return Results.BadRequest("Prompt cannot be empty.");
            }

            if (request.TargetModels.Count == 0)
            {
                return Results.BadRequest("At least one target model is required.");
            }

            if (!PortalBackendProxy.TryGetBackendBaseUri(configuration, "AiGateway", out var aiGatewayBaseUri))
            {
                return PortalBackendProxy.BackendNotConfigured("AiGateway");
            }

            var results = new ConcurrentBag<ModelCompareResult>();

            var tasks = request.TargetModels.Select(async modelId =>
            {
                var sw = Stopwatch.StartNew();
                using var outbound = PortalBackendProxy.CreateRequest(
                    httpContext,
                    HttpMethod.Post,
                    aiGatewayBaseUri,
                    "/api/v1/ai/chat/completions");

                outbound.Content = JsonContent.Create(new
                {
                    model = modelId,
                    temperature = request.Temperature,
                    messages = new[]
                    {
                        new { role = "user", content = request.Prompt }
                    }
                });

                using var response = await httpClientFactory.CreateClient().SendAsync(outbound, cancellationToken);
                string responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                sw.Stop();

                results.Add(new ModelCompareResult(
                    modelId,
                    response.Headers.TryGetValues("X-AI-Provider", out var providers) ? providers.FirstOrDefault() : null,
                    response.IsSuccessStatusCode ? responseText : $"Upstream gateway returned {(int)response.StatusCode}: {responseText}",
                    sw.ElapsedMilliseconds,
                    TryGetIntHeader(response, "X-Usage-Prompt-Tokens"),
                    TryGetIntHeader(response, "X-Usage-Completion-Tokens"),
                    TryGetDecimalHeader(response, "X-Estimated-Cost-Usd"),
                    null
                ));
            });

            await Task.WhenAll(tasks);

            return Results.Ok(results.OrderBy(r => r.ModelId, StringComparer.OrdinalIgnoreCase));
        })
        .WithName("CompareModels")
        .WithSummary("Concurrently execute a prompt across selected AI models and return side-by-side quality, latency, and cost metrics.");

        group.MapGet("/quotas/summary", async (
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            return PortalBackendProxy.TryGetBackendBaseUri(configuration, "CostOptimization", out var costBaseUri)
                ? await PortalBackendProxy.ForwardJsonAsync(httpContext, httpClientFactory, costBaseUri, "/api/v1/cost/budgets", cancellationToken)
                : PortalBackendProxy.BackendNotConfigured("CostOptimization");
        })
        .WithName("GetQuotaSummary")
        .WithSummary("Get current tenant token budget and quota utilization status.");

        return endpoints;
    }

    private static int? TryGetIntHeader(HttpResponseMessage response, string headerName)
        => response.Headers.TryGetValues(headerName, out var values)
           && int.TryParse(values.FirstOrDefault(), out var value)
            ? value
            : null;

    private static decimal? TryGetDecimalHeader(HttpResponseMessage response, string headerName)
        => response.Headers.TryGetValues(headerName, out var values)
           && decimal.TryParse(values.FirstOrDefault(), out var value)
            ? value
            : null;
}

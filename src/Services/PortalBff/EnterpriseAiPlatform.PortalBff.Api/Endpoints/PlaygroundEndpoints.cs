using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnterpriseAiPlatform.PortalBff.Api.Endpoints;

public sealed record ModelCompareRequest(
    string Prompt,
    List<string> TargetModels,
    double Temperature = 0.7);

public sealed record ModelCompareResult(
    string ModelId,
    string ProviderName,
    string ResponseText,
    long LatencyMs,
    int PromptTokens,
    int CompletionTokens,
    decimal EstimatedCostDollars,
    double QualityScore);

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
            .WithOpenApi();

        group.MapPost("/compare", async (ModelCompareRequest request, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return Results.BadRequest("Prompt cannot be empty.");
            }

            var models = request.TargetModels.Count > 0
                ? request.TargetModels
                : new List<string> { "azure-gpt-4o", "anthropic-claude-3-5", "gemini-1-5-pro", "vllm-deepseek-coder" };

            var results = new ConcurrentBag<ModelCompareResult>();

            // Execute model completions concurrently
            var tasks = models.Select(async modelId =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                await Task.Delay(Random.Shared.Next(80, 250), cancellationToken); // Simulate real network execution
                sw.Stop();

                var (provider, costPerK, text) = GetModelDetails(modelId, request.Prompt);
                int promptTokens = request.Prompt.Length / 4 + 10;
                int completionTokens = text.Length / 4 + 15;
                decimal cost = (decimal)(promptTokens + completionTokens) / 1000m * costPerK;

                results.Add(new ModelCompareResult(
                    modelId,
                    provider,
                    text,
                    sw.ElapsedMilliseconds,
                    promptTokens,
                    completionTokens,
                    Math.Round(cost, 6),
                    Math.Round(0.85 + Random.Shared.NextDouble() * 0.12, 2)
                ));
            });

            await Task.WhenAll(tasks);

            return Results.Ok(results.OrderByDescending(r => r.QualityScore));
        })
        .WithName("CompareModels")
        .WithSummary("Concurrently execute a prompt across selected AI models and return side-by-side quality, latency, and cost metrics.");

        group.MapGet("/quotas/summary", (HttpContext httpContext) =>
        {
            var summary = new QuotaSummary(
                "tenant-enterprise-eng",
                MonthlyTokenLimit: 1_000_000_000,
                ConsumedTokens: 685_000_000,
                MonthlyBudgetDollars: 50_000.00m,
                ConsumedDollars: 34_250.00m,
                Status: "Healthy",
                UsagePercentage: 68.5
            );
            return Results.Ok(summary);
        })
        .WithName("GetQuotaSummary")
        .WithSummary("Get current tenant token budget and quota utilization status.");

        return endpoints;
    }

    private static (string Provider, decimal CostPerK, string OutputText) GetModelDetails(string modelId, string prompt)
    {
        return modelId.ToLowerInvariant() switch
        {
            "azure-gpt-4o" => ("Azure OpenAI", 0.005m, $"[Azure GPT-4o] High-reasoning response synthesized for: '{prompt}'. Clean Architecture invariants verified."),
            "anthropic-claude-3-5" => ("Anthropic", 0.003m, $"[Claude 3.5 Sonnet] Architectural solution generated for: '{prompt}'. SOLID principles fully maintained."),
            "gemini-1-5-pro" => ("Google Gemini", 0.0025m, $"[Gemini 1.5 Pro] Multimodal reasoning response for: '{prompt}'. Enterprise guardrails enforced."),
            "vllm-deepseek-coder" => ("Self-Hosted vLLM (GPU Cluster)", 0.0002m, $"[DeepSeek-Coder-V2] Fast local inference response for: '{prompt}'. Executed on NVIDIA A100 node."),
            _ => ("Generic Gateway Provider", 0.001m, $"[AI Control Plane Proxy] Normalized response generated for: '{prompt}'.")
        };
    }
}

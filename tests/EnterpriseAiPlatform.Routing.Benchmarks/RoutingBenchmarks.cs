using BenchmarkDotNet.Attributes;
using EnterpriseAiPlatform.Routing.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Contracts.Requests;
using EnterpriseAiPlatform.Routing.Domain;
using EnterpriseAiPlatform.Routing.Application.Selection;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Routing.Benchmarks;

[MemoryDiagnoser]
public class RoutingBenchmarks
{
    private RoutingEvaluator _evaluator = default!;
    private RoutingConfiguration _ruleBasedConfiguration = default!;
    private RoutingConfiguration _budgetConfiguration = default!;
    private RouteEvaluationRequest _routingRequest = default!;

    [GlobalSetup]
    public void Setup()
    {
        _evaluator = new RoutingEvaluator();

        var request = new RouteEvaluationRequest(
            Prompt: "Please review this critical migration plan and explain the rollback path with JSON output.",
            SystemPrompt: "Senior engineering assistant.",
            Department: "platform",
            Repository: "core-platform",
            TaskType: "planning",
            RequestedMode: null,
            RequiredCapabilities: ["chat", "json-mode"],
            MaxTokens: 256,
            MaxCostUsd: 0.02m,
            Metadata: new Dictionary<string, string> { ["latency_preference"] = "low" });

        _routingRequest = request;

        _ruleBasedConfiguration = BuildConfiguration(RoutingMode.RuleBased);
        _budgetConfiguration = BuildConfiguration(RoutingMode.Budget);
    }

    [Benchmark]
    public RoutingEvaluationResult RuleBasedRoute() => _evaluator.Evaluate(_ruleBasedConfiguration, _routingRequest);

    [Benchmark]
    public RoutingEvaluationResult BudgetRoute() => _evaluator.Evaluate(_budgetConfiguration, _routingRequest);

    private static RoutingConfiguration BuildConfiguration(RoutingMode mode)
    {
        var now = DateTimeOffset.UtcNow;
        return new RoutingConfiguration(
            RoutingConfigurationId.New(),
            TenantId.From(Guid.Parse("55555555-5555-5555-5555-555555555555")),
            "routing-benchmark",
            mode,
            true,
            [
                new RoutingModelProfile(
                    "gpt-4o",
                    RoutingProvider.OpenAI,
                    "gpt-4o",
                    "GPT-4o",
                    ["chat", "json-mode", "reasoning"],
                    0.005m,
                    0.015m,
                    "usd",
                    120,
                    240,
                    320,
                    128000,
                    99.9,
                    RoutingHealthStatus.Healthy,
                    true,
                    new Dictionary<string, string>()),
                new RoutingModelProfile(
                    "gpt-4o-mini",
                    RoutingProvider.OpenAI,
                    "gpt-4o-mini",
                    "GPT-4o Mini",
                    ["chat", "json-mode"],
                    0.002m,
                    0.004m,
                    "usd",
                    80,
                    140,
                    220,
                    64000,
                    99.95,
                    RoutingHealthStatus.Healthy,
                    true,
                    new Dictionary<string, string>()),
            ],
            [
                new RoutingRule(
                    "critical-platform",
                    100,
                    true,
                    ["platform"],
                    ["core-platform"],
                    ["critical", "rollback"],
                    [],
                    ["chat", "json-mode"],
                    RoutingRequestComplexity.Simple,
                    2048,
                    0.02m,
                    RoutingMode.Policy,
                    "gpt-4o")
            ],
            new Dictionary<string, RoutingScopeProfile>(StringComparer.OrdinalIgnoreCase)
            {
                ["platform"] = new RoutingScopeProfile(
                    "platform",
                    "gpt-4o",
                    ["gpt-4o"],
                    [RoutingProvider.OpenAI],
                    0.03m,
                    4096,
                    100m,
                    250000,
                    true,
                    10,
                    "platform scope"),
            },
            new Dictionary<string, RoutingScopeProfile>(StringComparer.OrdinalIgnoreCase)
            {
                ["core-platform"] = new RoutingScopeProfile(
                    "core-platform",
                    "gpt-4o",
                    ["gpt-4o", "gpt-4o-mini"],
                    [RoutingProvider.OpenAI],
                    0.03m,
                    4096,
                    100m,
                    250000,
                    true,
                    10,
                    "repository scope"),
            },
            new RoutingScoringWeights(1, 1, 1, 1, 1, 1, 1, 1),
            new RoutingDefaults("gpt-4o-mini", RoutingMode.RuleBased, 90, 5, 3),
            new Dictionary<string, string> { ["created"] = now.ToString("O") },
            now);
    }
}

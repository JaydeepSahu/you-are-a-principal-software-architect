using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Application.Selection;
using EnterpriseAiPlatform.Routing.Contracts.Requests;
using EnterpriseAiPlatform.Routing.Domain;
using EnterpriseAiPlatform.Routing.Infrastructure;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Routing.UnitTests;

public sealed class RoutingEngineTests
{
    private static readonly TenantId TenantId = TenantId.From(Guid.Parse("44444444-4444-4444-4444-444444444444"));

    [Fact]
    public async Task ConfigurationCrudRoundTripsThroughHandlers()
    {
        var repository = new InMemoryRoutingConfigurationRepository();
        var context = new FakeRequestContextAccessor(TenantId);

        var upsertHandler = new UpsertRoutingConfigurationHandler(repository, context);
        var getHandler = new GetRoutingConfigurationHandler(repository, context);
        var deleteHandler = new DeleteRoutingConfigurationHandler(repository, context);

        var writeResult = await upsertHandler.Handle(new UpsertRoutingConfigurationCommand(BuildRequest()), default);

        Assert.True(writeResult.IsSuccess);
        Assert.Equal("routing-defaults", writeResult.Value.Name);
        Assert.Equal(RoutingMode.RuleBased, writeResult.Value.Mode);
        Assert.Equal(1, writeResult.Value.Version);

        var getResult = await getHandler.Handle(new GetRoutingConfigurationQuery(), default);
        Assert.True(getResult.IsSuccess);
        Assert.Equal(writeResult.Value.Id, getResult.Value.Id);
        Assert.Equal(2, getResult.Value.Models.Count);

        var deleteResult = await deleteHandler.Handle(new DeleteRoutingConfigurationCommand(), default);
        Assert.True(deleteResult.IsSuccess);

        var missingResult = await getHandler.Handle(new GetRoutingConfigurationQuery(), default);
        Assert.True(missingResult.IsFailure);
        Assert.Equal("routing.not_found", missingResult.Error.Code);
    }

    [Fact]
    public void EvaluateRoutesByRuleDepartmentAndBudgetSignals()
    {
        var evaluator = new RoutingEvaluator();
        var configuration = BuildConfiguration(RoutingMode.Policy);
        var request = new RouteEvaluationRequest(
            Prompt: "Please review this critical migration plan and explain the rollback path.",
            SystemPrompt: "Senior engineering assistant.",
            Department: "platform",
            Repository: "core-platform",
            TaskType: "planning",
            RequestedMode: RoutingMode.Policy,
            RequiredCapabilities: ["chat", "json-mode"],
            MaxTokens: 256,
            MaxCostUsd: 0.02m,
            Metadata: new Dictionary<string, string> { ["latency_preference"] = "low" });

        var result = evaluator.Evaluate(configuration, request);

        Assert.Equal(RoutingMode.Policy, result.Mode);
        Assert.Equal(RoutingRequestCategory.Planning, result.Category);
        Assert.True(result.Complexity >= RoutingRequestComplexity.Simple);
        Assert.True(result.EstimatedInputTokens > 0);
        Assert.True(result.EstimatedOutputTokens >= 256);
        Assert.Equal("gpt-4o", result.SelectedModelKey);
        Assert.Equal("critical-platform", result.MatchedRule);
        Assert.Equal("platform", result.Department);
        Assert.Equal("core-platform", result.Repository);
        Assert.True(result.Alternates.Count > 0);
    }

    [Fact]
    public void EvaluateUsesRepositoryScopeAndBudgetFallback()
    {
        var evaluator = new RoutingEvaluator();
        var configuration = BuildConfiguration(RoutingMode.Budget);
        var request = new RouteEvaluationRequest(
            Prompt: "Summarize the incident report and keep the output concise.",
            SystemPrompt: null,
            Department: null,
            Repository: "docs-service",
            TaskType: "summary",
            RequestedMode: null,
            RequiredCapabilities: ["summarization"],
            MaxTokens: 128,
            MaxCostUsd: 0.005m,
            Metadata: null);

        var result = evaluator.Evaluate(configuration, request);

        Assert.Equal(RoutingMode.Budget, result.Mode);
        Assert.Equal(RoutingRequestCategory.Summarization, result.Category);
        Assert.Equal("gpt-4o-mini", result.SelectedModelKey);
        Assert.False(result.BudgetExceeded);
        Assert.True(result.BudgetHeadroomUsd is not null);
    }

    private static RoutingConfigurationRequest BuildRequest()
        => new(
            Name: "routing-defaults",
            Mode: RoutingMode.RuleBased,
            Enabled: true,
            Models:
            [
                new RoutingModelProfileRequest(
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
                new RoutingModelProfileRequest(
                    "gpt-4o-mini",
                    RoutingProvider.OpenAI,
                    "gpt-4o-mini",
                    "GPT-4o Mini",
                    ["chat", "json-mode", "summarization"],
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
            Rules:
            [
                new RoutingRuleRequest(
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
                    "gpt-4o"),
            ],
            Departments: new Dictionary<string, RoutingScopeProfileRequest>(StringComparer.OrdinalIgnoreCase)
            {
                ["platform"] = new RoutingScopeProfileRequest(
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
                    "platform team scope"),
            },
            Repositories: new Dictionary<string, RoutingScopeProfileRequest>(StringComparer.OrdinalIgnoreCase)
            {
                ["core-platform"] = new RoutingScopeProfileRequest(
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
                ["docs-service"] = new RoutingScopeProfileRequest(
                    "docs-service",
                    "gpt-4o-mini",
                    ["gpt-4o-mini"],
                    [RoutingProvider.OpenAI],
                    0.01m,
                    2048,
                    20m,
                    100000,
                    true,
                    5,
                    "documentation scope"),
            },
            Weights: new RoutingScoringWeightsRequest(1, 1, 1, 1, 1, 1, 1, 1),
            Defaults: new RoutingDefaultsRequest("gpt-4o-mini", RoutingMode.RuleBased, 90, 5, 3),
            Metadata: new Dictionary<string, string> { ["team"] = "platform" });

    private static RoutingConfiguration BuildConfiguration(RoutingMode mode)
    {
        var request = BuildRequest() with { Mode = mode };
        return new RoutingConfiguration(
            RoutingConfigurationId.New(),
            TenantId,
            request.Name,
            request.Mode,
            request.Enabled,
            request.Models.Select(MapModel).ToList(),
            request.Rules.Select(MapRule).ToList(),
            request.Departments?.ToDictionary(item => item.Key, item => MapScope(item.Value), StringComparer.OrdinalIgnoreCase),
            request.Repositories?.ToDictionary(item => item.Key, item => MapScope(item.Value), StringComparer.OrdinalIgnoreCase),
            new RoutingScoringWeights(
                request.Weights.CapabilityWeight,
                request.Weights.CostWeight,
                request.Weights.LatencyWeight,
                request.Weights.AvailabilityWeight,
                request.Weights.HealthWeight,
                request.Weights.ContextHeadroomWeight,
                request.Weights.BudgetWeight,
                request.Weights.RuleWeight),
            new RoutingDefaults(
                request.Defaults.FallbackModelKey,
                request.Defaults.DefaultMode,
                request.Defaults.MinimumAvailabilityPercent,
                request.Defaults.MaxCandidates,
                request.Defaults.MaxAlternates),
            request.Metadata,
            DateTimeOffset.UtcNow);
    }

    private static RoutingModelProfile MapModel(RoutingModelProfileRequest request)
        => new(
            request.ModelKey,
            request.Provider,
            request.ProviderModelName,
            request.DisplayName,
            request.Capabilities,
            request.InputTokenCostPer1K,
            request.OutputTokenCostPer1K,
            request.Currency,
            request.P50Ms,
            request.P95Ms,
            request.P99Ms,
            request.ContextSize,
            request.AvailabilityPercent,
            request.Health,
            request.Enabled,
            request.Metadata);

    private static RoutingRule MapRule(RoutingRuleRequest request)
        => new(
            request.Name,
            request.Priority,
            request.Enabled,
            request.RequiredDepartments,
            request.RequiredRepositories,
            request.IncludeKeywords,
            request.ExcludeKeywords,
            request.RequiredCapabilities,
            request.MinimumComplexity,
            request.MaximumEstimatedTokens,
            request.MaximumEstimatedCostUsd,
            request.ModeOverride,
            request.SelectedModelKey);

    private static RoutingScopeProfile MapScope(RoutingScopeProfileRequest request)
        => new(
            request.Name,
            request.PreferredModelKey,
            request.AllowedModelKeys,
            request.AllowedProviders,
            request.MaximumEstimatedCostUsd,
            request.MaximumEstimatedTokens,
            request.MonthlyBudgetUsd,
            request.MonthlyTokenBudget,
            request.Enabled,
            request.Priority,
            request.Notes);

    private sealed class FakeRequestContextAccessor(TenantId tenantId) : IRequestContextAccessor
    {
        public RequestContext Current { get; } = new(
            tenantId,
            "test-correlation",
            "test-user",
            "test-app");
    }
}

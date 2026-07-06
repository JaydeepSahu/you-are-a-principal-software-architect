using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Evaluation.Application;
using EnterpriseAiPlatform.Evaluation.Application.Evaluations;
using EnterpriseAiPlatform.Evaluation.Contracts.Requests;
using EnterpriseAiPlatform.Evaluation.Domain;
using EnterpriseAiPlatform.Evaluation.Infrastructure;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Evaluation.UnitTests;

public sealed class EvaluationTests
{
    private static readonly TenantId TenantId = TenantId.From(Guid.Parse("22222222-2222-2222-2222-222222222222"));

    [Fact]
    public async Task CreateAndRetrieveEvaluation()
    {
        var harness = CreateHarness();
        var request = new CreateEvaluationRequest(
            "func-abc-123",
            "Function",
            LatencyMs: 250.5,
            CompilationSuccess: true,
            LintScore: 0.95,
            AcceptanceRate: 0.88,
            Confidence: 0.92,
            Similarity: 0.78,
            HallucinationScore: 0.05,
            CostUsd: 0.0023,
            Prompt: "Write a hello world function",
            ExpectedOutput: "Hello, World!",
            ActualOutput: "Hello, World!");

        var createResult = await harness.Create.Handle(new CreateEvaluationCommand(request), CancellationToken.None);
        Assert.True(createResult.IsSuccess);

        var getResult = await harness.Get.Handle(new GetEvaluationQuery(createResult.Value.EvaluationId), CancellationToken.None);
        Assert.True(getResult.IsSuccess);
        Assert.Equal("func-abc-123", getResult.Value.TargetId);
        Assert.True(getResult.Value.Metrics.CompilationSuccess);
        Assert.Equal(0.88, getResult.Value.Metrics.AcceptanceRate);
    }

    [Fact]
    public async Task ListEvaluationsReturnsPagedResults()
    {
        var harness = CreateHarness();

        for (var i = 0; i < 5; i++)
        {
            await harness.Create.Handle(new CreateEvaluationCommand(new CreateEvaluationRequest(
                $"target-{i}", "Function", LatencyMs: 100, CompilationSuccess: true,
                LintScore: 0.9, AcceptanceRate: 0.8, Confidence: 0.85, Similarity: 0.75,
                HallucinationScore: 0.1, CostUsd: 0.001)), CancellationToken.None);
        }

        var result = await harness.List.Handle(
            new ListEvaluationsQuery(new ListEvaluationsRequest(Page: 1, PageSize: 3)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Count);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(3, result.Value.PageSize);
    }

    [Fact]
    public async Task GetStatsAggregatesMetricsAcrossEvaluations()
    {
        var harness = CreateHarness();

        await harness.Create.Handle(new CreateEvaluationCommand(new CreateEvaluationRequest(
            "target-1", "Function", LatencyMs: 100, CompilationSuccess: true,
            LintScore: 0.8, AcceptanceRate: 0.8, Confidence: 0.9, Similarity: 0.7,
            HallucinationScore: 0.1, CostUsd: 0.001)), CancellationToken.None);

        await harness.Create.Handle(new CreateEvaluationCommand(new CreateEvaluationRequest(
            "target-2", "Function", LatencyMs: 200, CompilationSuccess: false,
            LintScore: 0.6, AcceptanceRate: 0.6, Confidence: 0.7, Similarity: 0.5,
            HallucinationScore: 0.3, CostUsd: 0.002)), CancellationToken.None);

        var stats = await harness.Stats.Handle(
            new GetEvaluationStatsQuery(),
            CancellationToken.None);

        Assert.True(stats.IsSuccess);
        Assert.Equal(2, stats.Value.TotalEvaluations);
        Assert.Equal(150, stats.Value.AvgLatencyMs);
        Assert.Equal(0.5, stats.Value.CompilationSuccessRate);
        Assert.Equal(0.7, stats.Value.AvgAcceptanceRate);
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundForMissingEvaluation()
    {
        var harness = CreateHarness();
        var result = await harness.Get.Handle(new GetEvaluationQuery(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal("evaluation.not_found", result.Error.Code);
    }

    [Fact]
    public void MetricsEnforceClampValidation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EvaluationMetrics(
            -1, false, null, 0, 0, 0, null, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EvaluationMetrics(
            100, false, 1.5, 0, 0, 0, null, null));
    }

    private static Harness CreateHarness()
    {
        var store = new InMemoryEvaluationStore();
        var context = new StaticRequestContextAccessor(new RequestContext(TenantId, "test", "user", "tests"));
        var create = new CreateEvaluationHandler(store, context);
        var get = new GetEvaluationHandler(store);
        var list = new ListEvaluationsHandler(store, context);
        var stats = new GetEvaluationStatsHandler(store, context);
        return new Harness(create, get, list, stats);
    }

    private sealed record Harness(
        CreateEvaluationHandler Create,
        GetEvaluationHandler Get,
        ListEvaluationsHandler List,
        GetEvaluationStatsHandler Stats);

    private sealed class StaticRequestContextAccessor(RequestContext ctx) : IRequestContextAccessor
    {
        public RequestContext Current => ctx;
    }
}

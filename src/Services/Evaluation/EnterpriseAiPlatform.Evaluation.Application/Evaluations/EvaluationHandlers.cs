using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Evaluation.Application.Abstractions;
using EnterpriseAiPlatform.Evaluation.Contracts.Requests;
using EnterpriseAiPlatform.Evaluation.Contracts.Responses;
using EvaluationDomain = EnterpriseAiPlatform.Evaluation.Domain;
using EvaluationErrors = EnterpriseAiPlatform.Evaluation.Application.EvaluationErrors;
using SharedKernel = EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Evaluation.Application.Evaluations;

public sealed class CreateEvaluationHandler(
    IEvaluationStore store,
    IRequestContextAccessor requestContext)
    : ICommandHandler<CreateEvaluationCommand, CreateEvaluationResponse>
{
    public async Task<SharedKernel.Result<CreateEvaluationResponse>> Handle(
        CreateEvaluationCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var metrics = new EvaluationDomain.EvaluationMetrics(
            command.Request.LatencyMs,
            command.Request.CompilationSuccess,
            command.Request.LintScore,
            command.Request.AcceptanceRate,
            command.Request.Confidence,
            command.Request.Similarity,
            command.Request.HallucinationScore,
            command.Request.CostUsd);

        var evaluation = new EvaluationDomain.Evaluation(
            EvaluationDomain.EvaluationId.New(),
            tenantId,
            command.Request.TargetId,
            command.Request.TargetType,
            metrics,
            command.Request.Prompt,
            command.Request.ExpectedOutput,
            command.Request.ActualOutput);

        await store.AddAsync(evaluation, cancellationToken);

        return SharedKernel.Result.Success(new CreateEvaluationResponse(
            evaluation.Id.Value,
            evaluation.EvaluatedAtUtc));
    }
}

public sealed class GetEvaluationHandler(
    IEvaluationStore store)
    : IQueryHandler<GetEvaluationQuery, EvaluationResponse>
{
    public async Task<SharedKernel.Result<EvaluationResponse>> Handle(
        GetEvaluationQuery query,
        CancellationToken cancellationToken)
    {
        var evaluation = await store.GetByIdAsync(EvaluationDomain.EvaluationId.From(query.EvaluationId), cancellationToken);
        if (evaluation is null)
        {
            return SharedKernel.Result.Failure<EvaluationResponse>(EvaluationErrors.NotFound(query.EvaluationId));
        }

        return SharedKernel.Result.Success(Map(evaluation));
    }

    private static EvaluationResponse Map(EvaluationDomain.Evaluation e) => new(
        e.Id.Value,
        e.TargetId,
        e.TargetType,
        new EvaluationMetricsResponse(
            e.Metrics.LatencyMs,
            e.Metrics.CompilationSuccess,
            e.Metrics.LintScore,
            e.Metrics.AcceptanceRate,
            e.Metrics.Confidence,
            e.Metrics.Similarity,
            e.Metrics.HallucinationScore,
            e.Metrics.CostUsd),
        e.Prompt,
        e.ExpectedOutput,
        e.ActualOutput,
        e.EvaluatedAtUtc);
}

public sealed class ListEvaluationsHandler(
    IEvaluationStore store,
    IRequestContextAccessor requestContext)
    : IQueryHandler<ListEvaluationsQuery, ListEvaluationsResponse>
{
    public async Task<SharedKernel.Result<ListEvaluationsResponse>> Handle(
        ListEvaluationsQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var pageSize = Math.Clamp(query.Request.PageSize, 1, 100);
        var page = Math.Max(query.Request.Page, 1);

        var items = await store.ListAsync(
            tenantId,
            query.Request.TargetId,
            query.Request.TargetType,
            query.Request.From,
            query.Request.To,
            page,
            pageSize,
            cancellationToken);

        var total = await store.CountAsync(
            tenantId,
            query.Request.TargetId,
            query.Request.TargetType,
            query.Request.From,
            query.Request.To,
            cancellationToken);

        return SharedKernel.Result.Success(new ListEvaluationsResponse(
            items.Select(Map).ToList(),
            total,
            page,
            pageSize));
    }

    private static EvaluationResponse Map(EvaluationDomain.Evaluation e) => new(
        e.Id.Value,
        e.TargetId,
        e.TargetType,
        new EvaluationMetricsResponse(
            e.Metrics.LatencyMs,
            e.Metrics.CompilationSuccess,
            e.Metrics.LintScore,
            e.Metrics.AcceptanceRate,
            e.Metrics.Confidence,
            e.Metrics.Similarity,
            e.Metrics.HallucinationScore,
            e.Metrics.CostUsd),
        e.Prompt,
        e.ExpectedOutput,
        e.ActualOutput,
        e.EvaluatedAtUtc);
}

public sealed class GetEvaluationStatsHandler(
    IEvaluationStore store,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetEvaluationStatsQuery, EvaluationStatsResponse>
{
    public async Task<SharedKernel.Result<EvaluationStatsResponse>> Handle(
        GetEvaluationStatsQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        const int pageSize = 1000;

        var allEvaluations = new List<EvaluationDomain.Evaluation>();
        var page = 1;

        while (true)
        {
            var batch = await store.ListAsync(
                tenantId,
                query.TargetId,
                query.TargetType,
                query.From,
                query.To,
                page,
                pageSize,
                cancellationToken);

            if (batch.Count == 0)
            {
                break;
            }

            allEvaluations.AddRange(batch);
            if (batch.Count < pageSize)
            {
                break;
            }

            page++;
        }

        if (allEvaluations.Count == 0)
        {
            return SharedKernel.Result.Success(new EvaluationStatsResponse(
                0, 0, 0, 0, 0, 0, 0, 0, 0));
        }

        var m = allEvaluations.Select(e => e.Metrics).ToList();
        var compilationSuccessCount = m.Count(x => x.CompilationSuccess);

        return SharedKernel.Result.Success(new EvaluationStatsResponse(
            allEvaluations.Count,
            Math.Round(m.Average(x => x.LatencyMs), 2),
            Math.Round(compilationSuccessCount / (double)allEvaluations.Count, 4),
            m.Where(x => x.LintScore.HasValue).Select(x => x.LintScore!.Value).DefaultIfEmpty(0).Average(),
            Math.Round(m.Average(x => x.AcceptanceRate), 4),
            Math.Round(m.Average(x => x.Confidence), 4),
            Math.Round(m.Average(x => x.Similarity), 4),
            m.Where(x => x.HallucinationScore.HasValue).Select(x => x.HallucinationScore!.Value).DefaultIfEmpty(0).Average(),
            Math.Round(m.Where(x => x.CostUsd.HasValue).Select(x => x.CostUsd!.Value).DefaultIfEmpty(0).Sum(), 6)));
    }
}

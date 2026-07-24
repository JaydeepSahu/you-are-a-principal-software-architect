using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Observability.Application.Abstractions;
using EnterpriseAiPlatform.Observability.Application.ObservabilityEvents;
using EnterpriseAiPlatform.Observability.Contracts.Requests;
using EnterpriseAiPlatform.Observability.Contracts.Responses;
using EnterpriseAiPlatform.Observability.Domain;

namespace EnterpriseAiPlatform.Observability.Application.ObservabilityEvents;

public sealed class RecordTraceHandler(
    ITraceRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<RecordTraceCommand, TraceRecordedResponse>
{
    public async Task<Result<TraceRecordedResponse>> Handle(
        RecordTraceCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var trace = new Trace(
            Guid.NewGuid(),
            requestContext.Current.TenantId,
            command.Request.TraceId,
            command.Request.SpanId,
            command.Request.Name,
            command.Request.Severity,
            command.Request.Service,
            command.Request.Attributes ?? new Dictionary<string, string>(),
            now,
            command.Request.DurationMs,
            command.Request.ErrorMessage);

        await repository.AddAsync(trace, cancellationToken);
        return Result.Success(new TraceRecordedResponse(trace.Id, now));
    }
}

public sealed class QueryTracesHandler(
    ITraceRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<QueryTracesQuery, TraceListResponse>
{
    public async Task<Result<TraceListResponse>> Handle(
        QueryTracesQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Request.Page, 1);
        var pageSize = Math.Clamp(query.Request.PageSize, 1, 500);
        var skip = (page - 1) * pageSize;

        var traces = await repository.QueryAsync(
            requestContext.Current.TenantId,
            query.Request.TraceId,
            query.Request.Service,
            query.Request.MinSeverity,
            query.Request.From,
            query.Request.To,
            skip,
            pageSize,
            cancellationToken);

        var total = await repository.CountAsync(
            requestContext.Current.TenantId,
            query.Request.TraceId,
            query.Request.Service,
            query.Request.MinSeverity,
            query.Request.From,
            query.Request.To,
            cancellationToken);

        return Result.Success(new TraceListResponse(
            total, page, pageSize,
            traces.Select(t => new TraceResponse(
                t.Id,
                t.TraceIdValue,
                t.SpanIdValue,
                t.Name,
                t.Severity.ToString(),
                t.Service,
                t.Attributes,
                t.DurationMs,
                t.IsError,
                t.StartedAtUtc)).ToList()));
    }
}

public sealed class GetHealthSummaryHandler(
    ITraceRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetHealthSummaryQuery, HealthSummaryResponse>
{
    public async Task<Result<HealthSummaryResponse>> Handle(
        GetHealthSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to = DateTimeOffset.UtcNow;

        var traces = await repository.QueryAsync(
            requestContext.Current.TenantId, null, null, null, from, to, 0, 1000, cancellationToken);

        var sorted = traces.OrderBy(t => t.DurationMs).ToList();
        var total = sorted.Count;
        var errorCount = sorted.Count(t => t.IsError);

        return Result.Success(new HealthSummaryResponse(
            total,
            errorCount,
            total == 0 ? 0 : Math.Round(errorCount / (double)total * 100, 2),
            total == 0 ? 0 : sorted[total / 2].DurationMs,
            total == 0 ? 0 : sorted[(int)(total * 0.95)].DurationMs,
            total == 0 ? 0 : sorted[(int)(total * 0.99)].DurationMs,
            DateTimeOffset.UtcNow));
    }
}

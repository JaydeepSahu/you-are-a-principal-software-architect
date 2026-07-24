using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Metering.Application.Abstractions;
using EnterpriseAiPlatform.Metering.Application.MeteringEvents;
using EnterpriseAiPlatform.Metering.Contracts.Requests;
using EnterpriseAiPlatform.Metering.Contracts.Responses;
using EnterpriseAiPlatform.Metering.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Metering.Application.MeteringEvents;

public sealed class RecordMeteringHandler(
    IMeteringRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<RecordMeteringCommand, MeteringRecordedResponse>
{
    public Task<Result<MeteringRecordedResponse>> Handle(
        RecordMeteringCommand command,
        CancellationToken cancellationToken)
    {
        var dimension = Enum.TryParse<MeteringDimension>(command.Request.Dimension, true, out var dim)
            ? dim
            : MeteringDimension.TokenPrompt;

        var record = new MeteringRecord(
            MeteringRecordId.New(),
            requestContext.Current.TenantId,
            command.Request.Provider,
            command.Request.Model,
            dimension,
            command.Request.Value,
            DateTimeOffset.UtcNow);

        _ = repository.AddAsync(record, cancellationToken);
        return Task.FromResult(Result.Success(new MeteringRecordedResponse(
            record.Id.Value,
            record.RecordedAtUtc)));
    }
}

public sealed class QueryMeteringReportHandler(
    IMeteringRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<QueryMeteringReportQuery, MeteringUsageResponse>
{
    public async Task<Result<MeteringUsageResponse>> Handle(
        QueryMeteringReportQuery query,
        CancellationToken cancellationToken)
    {
        var from = query.Request.From ?? DateTimeOffset.UtcNow.AddDays(-30);
        var to = query.Request.To ?? DateTimeOffset.UtcNow;

        var records = await repository.QueryAsync(
            requestContext.Current.TenantId,
            query.Request.Provider,
            query.Request.Model,
            from,
            to,
            cancellationToken);

        var breakdown = records
            .GroupBy(r => (r.Provider, r.Model))
            .Select(g => new MeteringBreakdownResponse(
                g.Key.Provider,
                g.Key.Model,
                (long)g.Where(r => r.Dimension == MeteringDimension.TokenCompletion).Sum(r => r.Value),
                (long)g.Where(r => r.Dimension == MeteringDimension.TokenPrompt).Sum(r => r.Value),
                (long)g.Where(r => r.Dimension == MeteringDimension.RequestCount).Sum(r => r.Value),
                EstimateCost(g)))
            .ToList();

        return Result.Success(new MeteringUsageResponse(
            (long)records.Where(r => r.Dimension == MeteringDimension.TokenCompletion).Sum(r => r.Value),
            (long)records.Where(r => r.Dimension == MeteringDimension.TokenPrompt).Sum(r => r.Value),
            (long)records.Where(r => r.Dimension == MeteringDimension.RequestCount).Sum(r => r.Value),
            (long)records.Where(r => r.Dimension == MeteringDimension.ErrorCount).Sum(r => r.Value),
            breakdown.Sum(b => b.CostUsd),
            from,
            to,
            breakdown));
    }

    private static double EstimateCost(IGrouping<(string, string), MeteringRecord> group)
        => group.Where(r => r.Dimension == MeteringDimension.TokenCompletion).Sum(r => r.Value) * 0.00001
         + group.Where(r => r.Dimension == MeteringDimension.TokenPrompt).Sum(r => r.Value) * 0.000003;
}

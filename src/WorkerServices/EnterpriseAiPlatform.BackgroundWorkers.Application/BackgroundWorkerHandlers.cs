using EnterpriseAiPlatform.BackgroundWorkers.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseAiPlatform.BackgroundWorkers.Application;

public partial class MeteringAggregationHandler(
    ILogger<MeteringAggregationHandler> logger)
    : INotificationHandler<MeteringAggregationCommand>
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Worker] Aggregating metering records for period {Start} to {End} (CorrelationId={CorrelationId})")]
    private static partial void LogMeteringAggregation(ILogger logger, DateTimeOffset start, DateTimeOffset end, Guid correlationId);

    public Task Handle(MeteringAggregationCommand notification, CancellationToken cancellationToken)
    {
        LogMeteringAggregation(logger, notification.PeriodStartUtc, notification.PeriodEndUtc, notification.CorrelationId);
        return Task.CompletedTask;
    }
}

public partial class CleanupExpiredPoliciesHandler(
    ILogger<CleanupExpiredPoliciesHandler> logger)
    : INotificationHandler<CleanupExpiredPoliciesCommand>
{
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "[Worker] Cleaning up expired policies (CorrelationId={CorrelationId})")]
    private static partial void LogCleanupExpiredPolicies(ILogger logger, Guid correlationId);

    public Task Handle(CleanupExpiredPoliciesCommand notification, CancellationToken cancellationToken)
    {
        LogCleanupExpiredPolicies(logger, notification.CorrelationId);
        return Task.CompletedTask;
    }
}

public partial class FlushAuditBufferHandler(
    ILogger<FlushAuditBufferHandler> logger)
    : INotificationHandler<FlushAuditBufferCommand>
{
    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "[Worker] Flushing audit buffer: batchSize={BatchSize} (CorrelationId={CorrelationId})")]
    private static partial void LogFlushAuditBuffer(ILogger logger, int batchSize, Guid correlationId);

    public Task Handle(FlushAuditBufferCommand notification, CancellationToken cancellationToken)
    {
        LogFlushAuditBuffer(logger, notification.BatchSize, notification.CorrelationId);
        return Task.CompletedTask;
    }
}

public partial class PurgeOldTracesHandler(
    ILogger<PurgeOldTracesHandler> logger)
    : INotificationHandler<PurgeOldTracesCommand>
{
    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "[Worker] Purging traces older than {RetentionDays} days (CorrelationId={CorrelationId})")]
    private static partial void LogPurgeOldTraces(ILogger logger, int retentionDays, Guid correlationId);

    public Task Handle(PurgeOldTracesCommand notification, CancellationToken cancellationToken)
    {
        LogPurgeOldTraces(logger, notification.RetentionDays, notification.CorrelationId);
        return Task.CompletedTask;
    }
}

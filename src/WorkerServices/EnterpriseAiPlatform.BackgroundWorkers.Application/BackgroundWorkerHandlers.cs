using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseAiPlatform.BackgroundWorkers.Application;

public sealed class MeteringAggregationHandler(
    ILogger<MeteringAggregationHandler> logger)
    : INotificationHandler<MeteringAggregationCommand>
{
    public Task Handle(MeteringAggregationCommand notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[Worker] Aggregating metering records for period {Start} to {End} (CorrelationId={CorrelationId})",
            notification.PeriodStartUtc,
            notification.PeriodEndUtc,
            notification.CorrelationId);
        return Task.CompletedTask;
    }
}

public sealed class CleanupExpiredPoliciesHandler(
    ILogger<CleanupExpiredPoliciesHandler> logger)
    : INotificationHandler<CleanupExpiredPoliciesCommand>
{
    public Task Handle(CleanupExpiredPoliciesCommand notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[Worker] Cleaning up expired policies (CorrelationId={CorrelationId})",
            notification.CorrelationId);
        return Task.CompletedTask;
    }
}

public sealed class FlushAuditBufferHandler(
    ILogger<FlushAuditBufferHandler> logger)
    : INotificationHandler<FlushAuditBufferCommand>
{
    public Task Handle(FlushAuditBufferCommand notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[Worker] Flushing audit buffer: batchSize={BatchSize} (CorrelationId={CorrelationId})",
            notification.BatchSize,
            notification.CorrelationId);
        return Task.CompletedTask;
    }
}

public sealed class PurgeOldTracesHandler(
    ILogger<PurgeOldTracesHandler> logger)
    : INotificationHandler<PurgeOldTracesCommand>
{
    public Task Handle(PurgeOldTracesCommand notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[Worker] Purging traces older than {RetentionDays} days (CorrelationId={CorrelationId})",
            notification.RetentionDays,
            notification.CorrelationId);
        return Task.CompletedTask;
    }
}

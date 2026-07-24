using MediatR;

namespace EnterpriseAiPlatform.BackgroundWorkers.Contracts;

public sealed record MeteringAggregationCommand(
    Guid CorrelationId,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndUtc) : INotification;

public sealed record CleanupExpiredPoliciesCommand(
    Guid CorrelationId) : INotification;

public sealed record FlushAuditBufferCommand(
    Guid CorrelationId,
    int BatchSize = 100) : INotification;

public sealed record PurgeOldTracesCommand(
    Guid CorrelationId,
    int RetentionDays = 7) : INotification;

namespace EnterpriseAiPlatform.BackgroundWorkers.Contracts;

public sealed record MeteringAggregationCommand(
    Guid CorrelationId,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndUtc);

public sealed record CleanupExpiredPoliciesCommand(
    Guid CorrelationId);

public sealed record FlushAuditBufferCommand(
    Guid CorrelationId,
    int BatchSize = 100);

public sealed record PurgeOldTracesCommand(
    Guid CorrelationId,
    int RetentionDays = 7);

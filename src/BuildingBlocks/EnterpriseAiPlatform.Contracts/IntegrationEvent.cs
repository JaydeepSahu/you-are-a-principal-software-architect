namespace EnterpriseAiPlatform.Contracts;

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    string EventType,
    int SchemaVersion);

public sealed record EventMetadata(
    Guid TenantId,
    string CorrelationId,
    string? CausationId,
    string? UserId,
    string? ApplicationId);

public sealed record EventEnvelope<TEvent>(TEvent Event, EventMetadata Metadata)
    where TEvent : IntegrationEvent;

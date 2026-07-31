namespace EnterpriseAiPlatform.Contracts.Events;

public sealed record AiRequestExecutedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    string Provider,
    string Model,
    int PromptTokens,
    int CompletionTokens,
    long DurationMs,
    bool IsSuccess,
    string? ErrorMessage,
    double CostUsd) : IntegrationEvent(EventId, OccurredAtUtc, nameof(AiRequestExecutedIntegrationEvent), 1);

public sealed record PolicyViolationDetectedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    string PolicyName,
    string ResourceType,
    string ViolationDetails,
    string ActionTaken) : IntegrationEvent(EventId, OccurredAtUtc, nameof(PolicyViolationDetectedIntegrationEvent), 1);

public sealed record ModelHealthStatusChangedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    string Provider,
    string ModelName,
    string OldStatus,
    string NewStatus,
    string? Reason) : IntegrationEvent(EventId, OccurredAtUtc, nameof(ModelHealthStatusChangedIntegrationEvent), 1);

namespace EnterpriseAiPlatform.Agents.Contracts.Responses;

public sealed record ApprovalRequestDto(
    Guid ApprovalId,
    Guid PlanId,
    Guid StepId,
    string ToolName,
    string ArgumentsJson,
    string Status,
    string? DecidedBy,
    string? DecisionReason,
    DateTimeOffset RequestedAt,
    DateTimeOffset? DecidedAt);

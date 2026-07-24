namespace EnterpriseAiPlatform.Agents.Contracts.Responses;

public sealed record PlanStepDto(
    Guid StepId,
    string StepName,
    string GoalDescription,
    string? ToolName,
    string? ArgumentsJson,
    string Status,
    string Mode,
    int ExecutionGroup,
    List<Guid> DependsOnStepIds,
    string? Output,
    string? ErrorMessage,
    int AttemptCount,
    bool RequiresApproval,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);

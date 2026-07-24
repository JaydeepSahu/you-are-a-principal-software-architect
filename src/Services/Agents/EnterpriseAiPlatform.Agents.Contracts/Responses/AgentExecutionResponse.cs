namespace EnterpriseAiPlatform.Agents.Contracts.Responses;

public sealed record AgentExecutionResponse(
    Guid PlanId,
    string AgentId,
    string Status,
    string UserGoal,
    List<PlanStepDto> Steps,
    Dictionary<string, string> WorkingMemory,
    Guid? PendingApprovalId = null,
    DateTimeOffset CreatedAt = default,
    DateTimeOffset? CompletedAt = null);

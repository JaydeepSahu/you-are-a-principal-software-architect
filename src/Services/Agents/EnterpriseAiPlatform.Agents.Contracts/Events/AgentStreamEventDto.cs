namespace EnterpriseAiPlatform.Agents.Contracts.Events;

public enum AgentEventType
{
    Thought = 0,
    StepStarted = 1,
    StepCompleted = 2,
    StepFailed = 3,
    ApprovalRequired = 4,
    OutputChunk = 5,
    PlanCompleted = 6,
    Error = 7
}

public sealed record AgentStreamEventDto(
    Guid PlanId,
    AgentEventType EventType,
    string Summary,
    string DetailsJson,
    DateTimeOffset Timestamp);

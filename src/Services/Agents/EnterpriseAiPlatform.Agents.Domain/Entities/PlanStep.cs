using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Domain.Entities;

public sealed class PlanStep : Entity<StepId>
{
    public string StepName { get; }
    public string GoalDescription { get; }
    public string? ToolName { get; }
    public string? ArgumentsJson { get; set; }
    public StepStatus Status { get; private set; }
    public ExecutionMode Mode { get; }
    public int ExecutionGroup { get; }
    public IReadOnlyList<StepId> DependsOnStepIds { get; }
    public string? Output { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxRetries { get; }
    public bool RequiresApproval { get; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public PlanStep(
        StepId id,
        string stepName,
        string goalDescription,
        string? toolName = null,
        string? argumentsJson = null,
        ExecutionMode mode = ExecutionMode.Sequential,
        int executionGroup = 0,
        IEnumerable<StepId>? dependsOnStepIds = null,
        bool requiresApproval = false,
        int maxRetries = 3)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        ArgumentException.ThrowIfNullOrWhiteSpace(goalDescription);

        StepName = stepName;
        GoalDescription = goalDescription;
        ToolName = toolName;
        ArgumentsJson = argumentsJson;
        Mode = mode;
        ExecutionGroup = executionGroup;
        DependsOnStepIds = (dependsOnStepIds ?? Enumerable.Empty<StepId>()).ToList().AsReadOnly();
        RequiresApproval = requiresApproval;
        MaxRetries = Math.Max(0, maxRetries);
        Status = StepStatus.Pending;
    }

    public void MarkRunning()
    {
        Status = StepStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        AttemptCount++;
    }

    public void MarkCompleted(string output)
    {
        Status = StepStatus.Completed;
        Output = output;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkWaitingForApproval()
    {
        Status = StepStatus.WaitingForApproval;
    }

    public void MarkFailed(string error)
    {
        ErrorMessage = error;
        if (AttemptCount >= MaxRetries)
        {
            Status = StepStatus.Failed;
            CompletedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            Status = StepStatus.Pending; // Retryable
        }
    }

    public void MarkCancelled()
    {
        Status = StepStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSkipped()
    {
        Status = StepStatus.Skipped;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}

using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.Events;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Domain.Aggregates;

public sealed class AgentPlan : AggregateRoot<PlanId>
{
    private readonly List<PlanStep> _steps = new();

    public AgentId AgentId { get; }
    public TenantId TenantId { get; }
    public string UserGoal { get; }
    public PlanStatus Status { get; private set; }
    public IReadOnlyList<PlanStep> Steps => _steps.AsReadOnly();
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public AgentPlan(PlanId id, AgentId agentId, TenantId tenantId, string userGoal)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userGoal);

        AgentId = agentId;
        TenantId = tenantId;
        UserGoal = userGoal;
        Status = PlanStatus.Created;
        CreatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new PlanCreatedEvent(Id, AgentId, TenantId));
    }

    public void AddStep(PlanStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _steps.Add(step);
    }

    public void MarkExecuting()
    {
        Status = PlanStatus.Executing;
    }

    public void MarkWaitingForApproval()
    {
        Status = PlanStatus.WaitingForApproval;
    }

    public void MarkCompleted()
    {
        Status = PlanStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new PlanCompletedEvent(Id, AgentId, true));
    }

    public void MarkFailed()
    {
        Status = PlanStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new PlanCompletedEvent(Id, AgentId, false));
    }

    public void MarkCancelled()
    {
        Status = PlanStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
        foreach (var step in _steps.Where(s => s.Status == StepStatus.Pending || s.Status == StepStatus.Running))
        {
            step.MarkCancelled();
        }
    }

    public IEnumerable<PlanStep> GetNextExecutableSteps()
    {
        if (Status != PlanStatus.Executing)
        {
            return Enumerable.Empty<PlanStep>();
        }

        var completedStepIds = _steps
            .Where(s => s.Status == StepStatus.Completed || s.Status == StepStatus.Skipped)
            .Select(s => s.Id)
            .ToHashSet();

        return _steps
            .Where(s => s.Status == StepStatus.Pending)
            .Where(s => s.DependsOnStepIds.All(dep => completedStepIds.Contains(dep)));
    }
}

using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Contracts.Responses;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.Agents.Application.Commands;

public sealed class ExecuteAgentPlanCommandHandler : IRequestHandler<ExecuteAgentPlanCommand, Result<AgentExecutionResponse>>
{
    private readonly IAgentPlanner _planner;
    private readonly IAgentExecutor _executor;
    private readonly IAgentMemoryStore _memoryStore;
    private readonly IApprovalManager _approvalManager;
    private readonly IAgentStreamBroadcaster _streamBroadcaster;

    public ExecuteAgentPlanCommandHandler(
        IAgentPlanner planner,
        IAgentExecutor executor,
        IAgentMemoryStore memoryStore,
        IApprovalManager approvalManager,
        IAgentStreamBroadcaster streamBroadcaster)
    {
        _planner = planner;
        _executor = executor;
        _memoryStore = memoryStore;
        _approvalManager = approvalManager;
        _streamBroadcaster = streamBroadcaster;
    }

    public async Task<Result<AgentExecutionResponse>> Handle(ExecuteAgentPlanCommand command, CancellationToken cancellationToken)
    {
        var agentId = AgentId.From(command.Request.AgentId);
        var planId = PlanId.New();
        var memory = await _memoryStore.GetOrCreateAsync(planId, command.TenantId, agentId, cancellationToken);

        if (command.Request.InitialWorkingMemory != null)
        {
            foreach (var (k, v) in command.Request.InitialWorkingMemory)
            {
                memory.SetWorkingState(k, v);
            }
        }

        var planResult = await _planner.CreatePlanAsync(agentId, command.TenantId, command.Request.Goal, memory, cancellationToken);
        if (planResult.IsFailure)
        {
            return Result.Failure<AgentExecutionResponse>(planResult.Error);
        }

        var plan = planResult.Value;

        Action<Contracts.Events.AgentStreamEventDto>? onStreamEvent = command.Request.StreamEvents
            ? evt => _streamBroadcaster.Broadcast(evt)
            : null;

        var execResult = await _executor.ExecutePlanAsync(plan, memory, onStreamEvent, cancellationToken);
        if (execResult.IsFailure)
        {
            return Result.Failure<AgentExecutionResponse>(execResult.Error);
        }

        await _memoryStore.SaveAsync(memory, cancellationToken);

        var pendingApproval = _approvalManager.GetPendingRequestsForPlan(plan.Id).FirstOrDefault();

        var stepDtos = plan.Steps.Select(s => new PlanStepDto(
            s.Id.Value,
            s.StepName,
            s.GoalDescription,
            s.ToolName,
            s.ArgumentsJson,
            s.Status.ToString(),
            s.Mode.ToString(),
            s.ExecutionGroup,
            s.DependsOnStepIds.Select(d => d.Value).ToList(),
            s.Output,
            s.ErrorMessage,
            s.AttemptCount,
            s.RequiresApproval,
            s.StartedAt,
            s.CompletedAt
        )).ToList();

        var response = new AgentExecutionResponse(
            plan.Id.Value,
            plan.AgentId.Value,
            plan.Status.ToString(),
            plan.UserGoal,
            stepDtos,
            memory.WorkingMemory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            pendingApproval?.Id.Value,
            plan.CreatedAt,
            plan.CompletedAt
        );

        return Result.Success(response);
    }
}

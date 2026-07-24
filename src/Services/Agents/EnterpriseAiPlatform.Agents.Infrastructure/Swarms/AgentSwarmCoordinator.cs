using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Contracts.Responses;
using EnterpriseAiPlatform.Agents.Domain.Aggregates;
using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Infrastructure.Swarms;

public sealed class AgentSwarmCoordinator : IAgentSwarmCoordinator
{
    private readonly IAgentExecutor _executor;
    private readonly IAgentMemoryStore _memoryStore;

    public AgentSwarmCoordinator(IAgentExecutor executor, IAgentMemoryStore memoryStore)
    {
        _executor = executor;
        _memoryStore = memoryStore;
    }

    public async Task<Result<SwarmExecutionResult>> ExecuteSwarmWorkflowAsync(
        TenantId tenantId,
        string leaderAgentId,
        string swarmGoal,
        List<SwarmTask> subTasks,
        CancellationToken cancellationToken = default)
    {
        var swarmId = Guid.NewGuid();
        var planId = PlanId.New();
        var leaderId = AgentId.From(leaderAgentId);
        var plan = new AgentPlan(planId, leaderId, tenantId, swarmGoal);

        var stepMap = new Dictionary<string, StepId>();
        foreach (var task in subTasks)
        {
            var stepId = StepId.New();
            stepMap[task.SubTaskId] = stepId;
        }

        foreach (var task in subTasks)
        {
            var depStepIds = task.DependsOnSubTaskIds
                .Where(dep => stepMap.ContainsKey(dep))
                .Select(dep => stepMap[dep])
                .ToList();

            var mode = depStepIds.Count > 0 ? ExecutionMode.Sequential : ExecutionMode.ParallelGroup;
            var step = new PlanStep(stepMap[task.SubTaskId], task.SubTaskId, task.Description, toolName: null, argumentsJson: "{}", mode: mode, executionGroup: 0, dependsOnStepIds: depStepIds);
            plan.AddStep(step);
        }

        var memory = await _memoryStore.GetOrCreateAsync(planId, tenantId, leaderId, cancellationToken);
        var execResult = await _executor.ExecutePlanAsync(plan, memory, null, cancellationToken);

        if (execResult.IsFailure)
        {
            return Result<SwarmExecutionResult>.Failure(execResult.Error);
        }

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

        var result = new SwarmExecutionResult(
            swarmId,
            leaderAgentId,
            plan.Status == PlanStatus.Completed,
            stepDtos,
            memory.WorkingMemory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );

        return Result<SwarmExecutionResult>.Success(result);
    }
}

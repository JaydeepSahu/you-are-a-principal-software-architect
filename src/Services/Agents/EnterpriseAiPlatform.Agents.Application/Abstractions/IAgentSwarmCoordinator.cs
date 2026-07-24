using EnterpriseAiPlatform.Agents.Contracts.Responses;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Application.Abstractions;

public sealed record SwarmTask(string SubTaskId, string AgentId, string Description, List<string> DependsOnSubTaskIds);

public sealed record SwarmExecutionResult(
    Guid SwarmId,
    string LeaderAgentId,
    bool Success,
    List<PlanStepDto> ExecutedSteps,
    Dictionary<string, string> ConsolidatedOutputs);

public interface IAgentSwarmCoordinator
{
    Task<Result<SwarmExecutionResult>> ExecuteSwarmWorkflowAsync(
        TenantId tenantId,
        string leaderAgentId,
        string swarmGoal,
        List<SwarmTask> subTasks,
        CancellationToken cancellationToken = default);
}

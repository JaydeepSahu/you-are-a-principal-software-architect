using EnterpriseAiPlatform.Agents.Domain.Aggregates;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Application.Abstractions;

public interface IAgentPlanner
{
    Task<Result<AgentPlan>> CreatePlanAsync(
        AgentId agentId,
        TenantId tenantId,
        string goal,
        AgentMemory memory,
        CancellationToken cancellationToken = default);
}

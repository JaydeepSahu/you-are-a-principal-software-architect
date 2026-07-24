using EnterpriseAiPlatform.Agents.Domain.Aggregates;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Application.Abstractions;

public interface IAgentMemoryStore
{
    Task<AgentMemory> GetOrCreateAsync(PlanId planId, TenantId tenantId, AgentId agentId, CancellationToken cancellationToken = default);
    Task SaveAsync(AgentMemory memory, CancellationToken cancellationToken = default);
}

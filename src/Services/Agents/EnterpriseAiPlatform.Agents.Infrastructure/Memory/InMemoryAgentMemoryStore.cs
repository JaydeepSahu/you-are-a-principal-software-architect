using System.Collections.Concurrent;
using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Domain.Aggregates;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Infrastructure.Memory;

public sealed class InMemoryAgentMemoryStore : IAgentMemoryStore
{
    private readonly ConcurrentDictionary<PlanId, AgentMemory> _memories = new();

    public Task<AgentMemory> GetOrCreateAsync(PlanId planId, TenantId tenantId, AgentId agentId, CancellationToken cancellationToken = default)
    {
        var memory = _memories.GetOrAdd(planId, pId => new AgentMemory(pId, tenantId, agentId));
        return Task.FromResult(memory);
    }

    public Task SaveAsync(AgentMemory memory, CancellationToken cancellationToken = default)
    {
        _memories[memory.Id] = memory;
        return Task.CompletedTask;
    }
}

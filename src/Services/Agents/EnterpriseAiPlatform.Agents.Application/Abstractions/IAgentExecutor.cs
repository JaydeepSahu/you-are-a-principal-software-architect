using EnterpriseAiPlatform.Agents.Contracts.Events;
using EnterpriseAiPlatform.Agents.Domain.Aggregates;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Application.Abstractions;

public interface IAgentExecutor
{
    Task<Result<AgentPlan>> ExecutePlanAsync(
        AgentPlan plan,
        AgentMemory memory,
        Action<AgentStreamEventDto>? onStreamEvent = null,
        CancellationToken cancellationToken = default);
}

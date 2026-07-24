using System.Threading.Channels;
using EnterpriseAiPlatform.Agents.Contracts.Events;

namespace EnterpriseAiPlatform.Agents.Application.Abstractions;

public interface IAgentStreamBroadcaster
{
    ChannelReader<AgentStreamEventDto> Subscribe(Guid planId);
    void Broadcast(AgentStreamEventDto streamEvent);
    void Complete(Guid planId);
}

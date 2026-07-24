using System.Collections.Concurrent;
using System.Threading.Channels;
using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Contracts.Events;

namespace EnterpriseAiPlatform.Agents.Infrastructure.Streaming;

public sealed class AgentStreamBroadcaster : IAgentStreamBroadcaster
{
    private readonly ConcurrentDictionary<Guid, Channel<AgentStreamEventDto>> _channels = new();

    public ChannelReader<AgentStreamEventDto> Subscribe(Guid planId)
    {
        var channel = _channels.GetOrAdd(planId, _ => Channel.CreateUnbounded<AgentStreamEventDto>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        }));

        return channel.Reader;
    }

    public void Broadcast(AgentStreamEventDto streamEvent)
    {
        if (_channels.TryGetValue(streamEvent.PlanId, out var channel))
        {
            channel.Writer.TryWrite(streamEvent);
        }
    }

    public void Complete(Guid planId)
    {
        if (_channels.TryRemove(planId, out var channel))
        {
            channel.Writer.TryComplete();
        }
    }
}

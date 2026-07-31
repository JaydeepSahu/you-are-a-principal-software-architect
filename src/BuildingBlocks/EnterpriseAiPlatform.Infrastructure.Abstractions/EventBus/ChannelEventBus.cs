using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using EnterpriseAiPlatform.Contracts;
using Microsoft.Extensions.Logging;

namespace EnterpriseAiPlatform.Infrastructure.Abstractions.EventBus;

[SuppressMessage("Reliability", "CA1848:Use the LoggerMessage delegates", Justification = "Event bus lifecycle logging.")]
public sealed class ChannelEventBus : IEventBus
{
    private readonly Channel<object> _channel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false
    });
    private readonly ILogger<ChannelEventBus> _logger;

    public ChannelEventBus(ILogger<ChannelEventBus> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(
        EventEnvelope<TEvent> envelope,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(envelope);

        await _channel.Writer.WriteAsync(envelope, cancellationToken);
        _logger.LogInformation("Published IntegrationEvent {EventType} ({EventId}) to local channel queue.",
            envelope.Event.EventType, envelope.Event.EventId);
    }
}

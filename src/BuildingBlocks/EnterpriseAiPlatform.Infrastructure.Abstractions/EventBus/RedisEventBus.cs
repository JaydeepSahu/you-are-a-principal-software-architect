using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using EnterpriseAiPlatform.Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EnterpriseAiPlatform.Infrastructure.Abstractions.EventBus;

[SuppressMessage("Reliability", "CA1848:Use the LoggerMessage delegates", Justification = "Event bus lifecycle logging.")]
public sealed class RedisEventBus : IEventBus
{
    private const string ChannelPrefix = "eap:events:";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RedisEventBus> _logger;

    public RedisEventBus(
        IConnectionMultiplexer redis,
        IServiceProvider serviceProvider,
        ILogger<RedisEventBus> logger)
    {
        _redis = redis;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(
        EventEnvelope<TEvent> envelope,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var channel = $"{ChannelPrefix}{envelope.Event.EventType}";
        var json = JsonSerializer.Serialize(envelope, JsonOptions);

        var subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync(new RedisChannel(channel, RedisChannel.PatternMode.Literal), json);

        _logger.LogInformation("Published IntegrationEvent {EventType} ({EventId}) to Redis channel {Channel}.",
            envelope.Event.EventType, envelope.Event.EventId, channel);
    }
}

using System.Diagnostics.CodeAnalysis;
using EnterpriseAiPlatform.Contracts;

namespace EnterpriseAiPlatform.Infrastructure.Abstractions.EventBus;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffixes", Justification = "Standard event handler naming convention.")]
public interface IIntegrationEventHandler<TEvent>
    where TEvent : IntegrationEvent
{
    Task HandleAsync(EventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default);
}

public interface IEventBus
{
    Task PublishAsync<TEvent>(
        EventEnvelope<TEvent> envelope,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}

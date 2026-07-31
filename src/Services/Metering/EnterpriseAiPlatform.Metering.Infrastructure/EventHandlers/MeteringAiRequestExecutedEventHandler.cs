using System.Diagnostics.CodeAnalysis;
using EnterpriseAiPlatform.Contracts;
using EnterpriseAiPlatform.Contracts.Events;
using EnterpriseAiPlatform.Infrastructure.Abstractions.EventBus;
using EnterpriseAiPlatform.Metering.Application.Abstractions;
using EnterpriseAiPlatform.Metering.Domain;

namespace EnterpriseAiPlatform.Metering.Infrastructure.EventHandlers;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffixes", Justification = "Standard event handler naming convention.")]
public sealed class MeteringAiRequestExecutedEventHandler(IMeteringRepository meteringRepository)
    : IIntegrationEventHandler<AiRequestExecutedIntegrationEvent>
{
    public async Task HandleAsync(
        EventEnvelope<AiRequestExecutedIntegrationEvent> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        var @event = envelope.Event;
        var metadata = envelope.Metadata;
        var tenantId = SharedKernel.TenantId.From(metadata.TenantId);

        if (@event.PromptTokens > 0)
        {
            var promptRecord = new MeteringRecord(
                MeteringRecordId.New(),
                tenantId,
                @event.Provider,
                @event.Model,
                MeteringDimension.TokenPrompt,
                @event.PromptTokens,
                @event.OccurredAtUtc);

            await meteringRepository.AddAsync(promptRecord, cancellationToken);
        }

        if (@event.CompletionTokens > 0)
        {
            var completionRecord = new MeteringRecord(
                MeteringRecordId.New(),
                tenantId,
                @event.Provider,
                @event.Model,
                MeteringDimension.TokenCompletion,
                @event.CompletionTokens,
                @event.OccurredAtUtc);

            await meteringRepository.AddAsync(completionRecord, cancellationToken);
        }
    }
}

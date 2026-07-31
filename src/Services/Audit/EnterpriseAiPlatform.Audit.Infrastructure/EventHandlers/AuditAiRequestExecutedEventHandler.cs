using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EnterpriseAiPlatform.Audit.Application.Abstractions;
using EnterpriseAiPlatform.Audit.Domain;
using EnterpriseAiPlatform.Contracts;
using EnterpriseAiPlatform.Contracts.Events;
using EnterpriseAiPlatform.Infrastructure.Abstractions.EventBus;

namespace EnterpriseAiPlatform.Audit.Infrastructure.EventHandlers;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffixes", Justification = "Standard event handler naming convention.")]
public sealed class AuditAiRequestExecutedEventHandler(IAuditRepository auditRepository)
    : IIntegrationEventHandler<AiRequestExecutedIntegrationEvent>
{
    public async Task HandleAsync(
        EventEnvelope<AiRequestExecutedIntegrationEvent> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        var @event = envelope.Event;
        var metadata = envelope.Metadata;

        var entry = new AuditEntry(
            AuditEntryId.New(),
            SharedKernel.TenantId.From(metadata.TenantId),
            AuditAction.ModelInvoked,
            @event.IsSuccess ? AuditSeverity.Info : AuditSeverity.Error,
            "Model",
            @event.Model,
            metadata.UserId,
            metadata.ApplicationId,
            metadata.CorrelationId,
            null,
            new Dictionary<string, string>
            {
                ["Provider"] = @event.Provider,
                ["PromptTokens"] = @event.PromptTokens.ToString(CultureInfo.InvariantCulture),
                ["CompletionTokens"] = @event.CompletionTokens.ToString(CultureInfo.InvariantCulture),
                ["DurationMs"] = @event.DurationMs.ToString(CultureInfo.InvariantCulture),
                ["CostUsd"] = @event.CostUsd.ToString("F6", CultureInfo.InvariantCulture)
            },
            @event.OccurredAtUtc);

        await auditRepository.AddAsync(entry, cancellationToken);
    }
}

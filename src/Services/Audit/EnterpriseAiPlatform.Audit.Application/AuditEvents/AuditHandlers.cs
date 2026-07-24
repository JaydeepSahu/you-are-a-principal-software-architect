using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Audit.Application.Abstractions;
using EnterpriseAiPlatform.Audit.Application.AuditEvents;
using EnterpriseAiPlatform.Audit.Contracts.Requests;
using EnterpriseAiPlatform.Audit.Contracts.Responses;
using EnterpriseAiPlatform.Audit.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Audit.Application.AuditEvents;

public sealed class RecordAuditHandler(
    IAuditRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<RecordAuditEventCommand, AuditEventRecordedResponse>
{
    public async Task<Result<AuditEventRecordedResponse>> Handle(
        RecordAuditEventCommand command,
        CancellationToken cancellationToken)
    {
        var ctx = requestContext.Current;
        var action = Enum.TryParse<AuditAction>(command.Request.Action, true, out var act) ? act : AuditAction.Execute;
        var severity = Enum.TryParse<AuditSeverity>(command.Request.Severity, true, out var sev) ? sev : AuditSeverity.Info;

        var entry = new AuditEntry(
            AuditEntryId.New(),
            ctx.TenantId,
            action,
            severity,
            command.Request.ResourceType,
            command.Request.ResourceId,
            command.Request.UserId ?? ctx.UserId,
            command.Request.ApplicationId ?? ctx.ApplicationId,
            ctx.CorrelationId,
            null,
            command.Request.Metadata ?? new Dictionary<string, string>(),
            DateTimeOffset.UtcNow);

        await repository.AddAsync(entry, cancellationToken);

        return Result.Success(new AuditEventRecordedResponse(
            entry.Id.Value,
            entry.OccurredAtUtc));
    }
}

public sealed class QueryAuditLogHandler(
    IAuditRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<QueryAuditLogQuery, AuditLogResponse>
{
    public async Task<Result<AuditLogResponse>> Handle(
        QueryAuditLogQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var pageSize = Math.Clamp(query.Request.PageSize, 1, 200);
        var page = Math.Max(query.Request.Page, 1);
        var skip = (page - 1) * pageSize;

        AuditAction? action = Enum.TryParse<AuditAction>(query.Request.Action, true, out var act) ? act : null;
        AuditSeverity? minSeverity = Enum.TryParse<AuditSeverity>(query.Request.MinSeverity, true, out var sev) ? sev : null;

        var entries = await repository.QueryAsync(
            tenantId,
            query.Request.ResourceType,
            query.Request.ResourceId,
            query.Request.UserId,
            action,
            minSeverity,
            query.Request.From,
            query.Request.To,
            skip,
            pageSize,
            cancellationToken);

        var total = await repository.CountAsync(
            tenantId,
            query.Request.ResourceType,
            query.Request.ResourceId,
            query.Request.UserId,
            action,
            minSeverity,
            query.Request.From,
            query.Request.To,
            cancellationToken);

        return Result.Success(new AuditLogResponse(
            total,
            page,
            pageSize,
            entries.Select(e => new AuditEntryResponse(
                e.Id.Value,
                e.Action.ToString(),
                e.Severity.ToString(),
                e.ResourceType,
                e.ResourceId,
                e.UserId,
                e.ApplicationId,
                e.CorrelationId,
                e.IpAddress,
                e.Metadata,
                e.OccurredAtUtc)).ToList()));
    }
}

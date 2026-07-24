using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Audit.Contracts.Requests;
using EnterpriseAiPlatform.Audit.Contracts.Responses;

namespace EnterpriseAiPlatform.Audit.Application.AuditEvents;

public sealed record RecordAuditEventCommand(RecordAuditEventRequest Request) : ICommand<AuditEventRecordedResponse>;

public sealed record QueryAuditLogQuery(QueryAuditLogRequest Request) : IQuery<AuditLogResponse>;

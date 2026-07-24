using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Observability.Contracts.Requests;
using EnterpriseAiPlatform.Observability.Contracts.Responses;

namespace EnterpriseAiPlatform.Observability.Application.ObservabilityEvents;

public sealed record RecordTraceCommand(RecordTraceRequest Request) : ICommand<TraceRecordedResponse>;
public sealed record QueryTracesQuery(QueryTracesRequest Request) : IQuery<TraceListResponse>;
public sealed record GetHealthSummaryQuery : IQuery<HealthSummaryResponse>;

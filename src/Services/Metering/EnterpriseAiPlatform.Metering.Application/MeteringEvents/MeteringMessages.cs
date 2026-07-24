using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Metering.Contracts.Requests;
using EnterpriseAiPlatform.Metering.Contracts.Responses;

namespace EnterpriseAiPlatform.Metering.Application.MeteringEvents;

public sealed record RecordMeteringCommand(RecordMeteringRequest Request) : ICommand<MeteringRecordedResponse>;
public sealed record QueryMeteringReportQuery(QueryMeteringReportRequest Request) : IQuery<MeteringUsageResponse>;

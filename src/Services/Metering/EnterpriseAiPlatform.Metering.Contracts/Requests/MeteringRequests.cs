namespace EnterpriseAiPlatform.Metering.Contracts.Requests;

public sealed record RecordMeteringRequest(
    string Provider,
    string Model,
    string Dimension,
    double Value);

public sealed record QueryMeteringReportRequest(
    string? Provider = null,
    string? Model = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null);

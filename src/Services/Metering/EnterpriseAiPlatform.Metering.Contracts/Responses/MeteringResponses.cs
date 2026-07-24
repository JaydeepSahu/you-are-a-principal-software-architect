namespace EnterpriseAiPlatform.Metering.Contracts.Responses;

public sealed record MeteringRecordedResponse(
    Guid RecordId,
    DateTimeOffset RecordedAtUtc);

public sealed record MeteringUsageResponse(
    long TokenCompletionTotal,
    long TokenPromptTotal,
    long RequestCountTotal,
    long ErrorCountTotal,
    double CostUsd,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndUtc,
    IReadOnlyList<MeteringBreakdownResponse> Breakdown);

public sealed record MeteringBreakdownResponse(
    string Provider,
    string Model,
    long TokenCompletionTotal,
    long TokenPromptTotal,
    long RequestCountTotal,
    double CostUsd);

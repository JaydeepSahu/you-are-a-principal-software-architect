namespace EnterpriseAiPlatform.CostOptimization.Contracts.Requests;

public sealed record CreateModelCostRequest(
    string ProviderId,
    string ModelId,
    string ModelName,
    decimal InputCostPerToken,
    decimal OutputCostPerToken,
    decimal CostPerRequest,
    decimal CostPerSecond,
    DateTimeOffset EffectiveFrom);

public sealed record UpdateModelCostRequest(
    decimal? InputCostPerToken,
    decimal? OutputCostPerToken,
    decimal? CostPerRequest,
    decimal? CostPerSecond,
    DateTimeOffset? EffectiveTo);

public sealed record QueryModelCostsRequest(
    string? ProviderId,
    string? ModelId,
    bool? IsActive,
    int Skip,
    int Take);

public sealed record CalculateCostRequest(
    string ProviderId,
    string ModelId,
    int InputTokens,
    int OutputTokens,
    double? ProcessingSeconds);

public sealed record RecordCostRequest(
    string UserId,
    string? DepartmentId,
    string ProviderId,
    string ModelId,
    string Category,
    decimal Amount,
    string Currency,
    int InputTokens,
    int OutputTokens,
    double? ProcessingSeconds,
    string RequestId,
    string? CorrelationId,
    Dictionary<string, string>? Metadata);

public sealed record QueryCostRecordsRequest(
    string? UserId,
    string? DepartmentId,
    string? ProviderId,
    string? ModelId,
    string? Category,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Skip,
    int Take);

public sealed record GetCostSummaryRequest(
    string? DepartmentId,
    string? UserId,
    int Year,
    int Month);

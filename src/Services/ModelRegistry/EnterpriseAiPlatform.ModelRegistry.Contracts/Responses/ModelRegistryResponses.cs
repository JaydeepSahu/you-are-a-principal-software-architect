using EnterpriseAiPlatform.ModelRegistry.Domain;

namespace EnterpriseAiPlatform.ModelRegistry.Contracts.Responses;

public sealed record ModelRegistryPageResponse(
    int Skip,
    int Take,
    int TotalCount,
    IReadOnlyList<ModelRegistryResponse> Items);

public sealed record ModelRegistryResponse(
    Guid Id,
    Guid TenantId,
    ModelProvider Provider,
    string ProviderModelName,
    string DisplayName,
    string? Description,
    IReadOnlyList<ModelCapabilityResponse> Capabilities,
    ModelPricingResponse Pricing,
    ModelLatencyResponse Latency,
    int ContextSize,
    ModelAvailabilityResponse Availability,
    ModelHealthResponse Health,
    IReadOnlyDictionary<string, string> Configuration,
    int Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ModelCapabilityResponse(
    string Name,
    string? Description,
    bool Enabled);

public sealed record ModelPricingResponse(
    decimal InputTokenCostPer1K,
    decimal OutputTokenCostPer1K,
    decimal? CachedInputTokenCostPer1K,
    string Currency);

public sealed record ModelLatencyResponse(
    double P50Ms,
    double P95Ms,
    double P99Ms,
    DateTimeOffset MeasuredAtUtc);

public sealed record ModelAvailabilityResponse(
    bool IsAvailable,
    double AvailabilityPercent,
    string? Region,
    DateTimeOffset LastCheckedUtc);

public sealed record ModelHealthResponse(
    ModelHealthStatus Status,
    string? Message,
    DateTimeOffset CheckedAtUtc);

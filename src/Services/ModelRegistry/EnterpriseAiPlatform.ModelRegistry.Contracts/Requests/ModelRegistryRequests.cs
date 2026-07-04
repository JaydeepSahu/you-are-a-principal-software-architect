using EnterpriseAiPlatform.ModelRegistry.Domain;

namespace EnterpriseAiPlatform.ModelRegistry.Contracts.Requests;

public sealed record ModelRegistryWriteRequest(
    ModelProvider Provider,
    string ProviderModelName,
    string DisplayName,
    string? Description,
    IReadOnlyList<ModelCapabilityRequest> Capabilities,
    ModelPricingRequest Pricing,
    ModelLatencyRequest Latency,
    int ContextSize,
    ModelAvailabilityRequest Availability,
    ModelHealthRequest Health,
    IReadOnlyDictionary<string, string>? Configuration);

public sealed record ModelCapabilityRequest(
    string Name,
    string? Description,
    bool Enabled = true);

public sealed record ModelPricingRequest(
    decimal InputTokenCostPer1K,
    decimal OutputTokenCostPer1K,
    string Currency,
    decimal? CachedInputTokenCostPer1K = null);

public sealed record ModelLatencyRequest(
    double P50Ms,
    double P95Ms,
    double P99Ms,
    DateTimeOffset MeasuredAtUtc);

public sealed record ModelAvailabilityRequest(
    bool IsAvailable,
    double AvailabilityPercent,
    string? Region,
    DateTimeOffset LastCheckedUtc);

public sealed record ModelHealthRequest(
    ModelHealthStatus Status,
    string? Message,
    DateTimeOffset CheckedAtUtc);

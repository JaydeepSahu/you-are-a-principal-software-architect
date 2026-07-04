using EnterpriseAiPlatform.Routing.Domain;

namespace EnterpriseAiPlatform.Routing.Contracts.Requests;

public sealed record RoutingConfigurationRequest(
    string Name,
    RoutingMode Mode,
    bool Enabled,
    IReadOnlyList<RoutingModelProfileRequest> Models,
    IReadOnlyList<RoutingRuleRequest> Rules,
    IReadOnlyDictionary<string, RoutingScopeProfileRequest>? Departments,
    IReadOnlyDictionary<string, RoutingScopeProfileRequest>? Repositories,
    RoutingScoringWeightsRequest Weights,
    RoutingDefaultsRequest Defaults,
    IReadOnlyDictionary<string, string>? Metadata);

public sealed record RoutingModelProfileRequest(
    string ModelKey,
    RoutingProvider Provider,
    string ProviderModelName,
    string DisplayName,
    IReadOnlyList<string> Capabilities,
    decimal InputTokenCostPer1K,
    decimal OutputTokenCostPer1K,
    string Currency,
    double P50Ms,
    double P95Ms,
    double P99Ms,
    int ContextSize,
    double AvailabilityPercent,
    RoutingHealthStatus Health,
    bool Enabled,
    IReadOnlyDictionary<string, string>? Metadata);

public sealed record RoutingRuleRequest(
    string Name,
    int Priority,
    bool Enabled,
    IReadOnlyList<string>? RequiredDepartments,
    IReadOnlyList<string>? RequiredRepositories,
    IReadOnlyList<string>? IncludeKeywords,
    IReadOnlyList<string>? ExcludeKeywords,
    IReadOnlyList<string>? RequiredCapabilities,
    RoutingRequestComplexity? MinimumComplexity,
    int? MaximumEstimatedTokens,
    decimal? MaximumEstimatedCostUsd,
    RoutingMode? ModeOverride,
    string? SelectedModelKey);

public sealed record RoutingScopeProfileRequest(
    string Name,
    string? PreferredModelKey,
    IReadOnlyList<string>? AllowedModelKeys,
    IReadOnlyList<RoutingProvider>? AllowedProviders,
    decimal? MaximumEstimatedCostUsd,
    int? MaximumEstimatedTokens,
    decimal? MonthlyBudgetUsd,
    int? MonthlyTokenBudget,
    bool Enabled,
    int Priority,
    string? Notes);

public sealed record RoutingScoringWeightsRequest(
    double CapabilityWeight,
    double CostWeight,
    double LatencyWeight,
    double AvailabilityWeight,
    double HealthWeight,
    double ContextHeadroomWeight,
    double BudgetWeight,
    double RuleWeight);

public sealed record RoutingDefaultsRequest(
    string? FallbackModelKey,
    RoutingMode DefaultMode,
    double MinimumAvailabilityPercent,
    int MaxCandidates,
    int MaxAlternates);

public sealed record RouteEvaluationRequest(
    string Prompt,
    string? SystemPrompt,
    string? Department,
    string? Repository,
    string? TaskType,
    RoutingMode? RequestedMode,
    IReadOnlyList<string>? RequiredCapabilities,
    int? MaxTokens,
    decimal? MaxCostUsd,
    IReadOnlyDictionary<string, string>? Metadata);

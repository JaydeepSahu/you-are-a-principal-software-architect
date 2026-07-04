using EnterpriseAiPlatform.Routing.Domain;

namespace EnterpriseAiPlatform.Routing.Contracts.Responses;

public sealed record RoutingConfigurationResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    RoutingMode Mode,
    bool Enabled,
    IReadOnlyList<RoutingModelProfileResponse> Models,
    IReadOnlyList<RoutingRuleResponse> Rules,
    IReadOnlyDictionary<string, RoutingScopeProfileResponse> Departments,
    IReadOnlyDictionary<string, RoutingScopeProfileResponse> Repositories,
    RoutingScoringWeightsResponse Weights,
    RoutingDefaultsResponse Defaults,
    IReadOnlyDictionary<string, string> Metadata,
    int Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record RoutingModelProfileResponse(
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
    IReadOnlyDictionary<string, string> Metadata);

public sealed record RoutingRuleResponse(
    string Name,
    int Priority,
    bool Enabled,
    IReadOnlyList<string> RequiredDepartments,
    IReadOnlyList<string> RequiredRepositories,
    IReadOnlyList<string> IncludeKeywords,
    IReadOnlyList<string> ExcludeKeywords,
    IReadOnlyList<string> RequiredCapabilities,
    RoutingRequestComplexity? MinimumComplexity,
    int? MaximumEstimatedTokens,
    decimal? MaximumEstimatedCostUsd,
    RoutingMode? ModeOverride,
    string? SelectedModelKey);

public sealed record RoutingScopeProfileResponse(
    string Name,
    string? PreferredModelKey,
    IReadOnlyList<string> AllowedModelKeys,
    IReadOnlyList<RoutingProvider> AllowedProviders,
    decimal? MaximumEstimatedCostUsd,
    int? MaximumEstimatedTokens,
    decimal? MonthlyBudgetUsd,
    int? MonthlyTokenBudget,
    bool Enabled,
    int Priority,
    string? Notes);

public sealed record RoutingScoringWeightsResponse(
    double CapabilityWeight,
    double CostWeight,
    double LatencyWeight,
    double AvailabilityWeight,
    double HealthWeight,
    double ContextHeadroomWeight,
    double BudgetWeight,
    double RuleWeight);

public sealed record RoutingDefaultsResponse(
    string? FallbackModelKey,
    RoutingMode DefaultMode,
    double MinimumAvailabilityPercent,
    int MaxCandidates,
    int MaxAlternates);

public sealed record RouteEvaluationResponse(
    RoutingMode Mode,
    RoutingRequestCategory Category,
    RoutingRequestComplexity Complexity,
    int ComplexityScore,
    int EstimatedInputTokens,
    int EstimatedOutputTokens,
    decimal EstimatedCostUsd,
    string SelectedModelKey,
    RoutingProvider SelectedProvider,
    string SelectedProviderModelName,
    string SelectedDisplayName,
    string Reason,
    string? MatchedRule,
    string? Department,
    string? Repository,
    bool BudgetExceeded,
    decimal? BudgetHeadroomUsd,
    IReadOnlyList<RoutingModelOptionResponse> Alternates,
    DateTimeOffset EvaluatedAtUtc);

public sealed record RoutingModelOptionResponse(
    string ModelKey,
    RoutingProvider Provider,
    string ProviderModelName,
    string DisplayName,
    double Score,
    decimal EstimatedCostUsd,
    int EstimatedTokens,
    double EstimatedLatencyMs,
    double AvailabilityPercent);

namespace EnterpriseAiPlatform.Routing.Domain;

public sealed record RoutingModelOption(
    string ModelKey,
    RoutingProvider Provider,
    string ProviderModelName,
    string DisplayName,
    double Score,
    decimal EstimatedCostUsd,
    int EstimatedTokens,
    double EstimatedLatencyMs,
    double AvailabilityPercent);

public sealed record RoutingDecision(
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
    IReadOnlyList<RoutingModelOption> Alternates,
    DateTimeOffset EvaluatedAtUtc);

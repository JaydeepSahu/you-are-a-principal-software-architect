using EnterpriseAiPlatform.Routing.Contracts.Responses;
using EnterpriseAiPlatform.Routing.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Routing.Application.Abstractions;

public interface IRoutingConfigurationRepository
{
    Task<RoutingConfiguration?> GetAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    Task UpsertAsync(RoutingConfiguration configuration, CancellationToken cancellationToken = default);

    Task DeleteAsync(TenantId tenantId, CancellationToken cancellationToken = default);
}

public interface IRoutingTelemetry
{
    void RecordEvaluation(RoutingMode mode, RoutingRequestCategory category, string selectedModelKey, TimeSpan elapsed);
}

public sealed record RoutingModelCandidate(
    string ModelKey,
    RoutingProvider Provider,
    string ProviderModelName,
    string DisplayName,
    double Score,
    decimal EstimatedCostUsd,
    int EstimatedTokens,
    double EstimatedLatencyMs,
    double AvailabilityPercent);

public sealed record RoutingEvaluationResult(
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
    IReadOnlyList<RoutingModelCandidate> Alternates,
    DateTimeOffset EvaluatedAtUtc);

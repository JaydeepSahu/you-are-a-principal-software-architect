using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Contracts.Requests;
using EnterpriseAiPlatform.Routing.Contracts.Responses;
using EnterpriseAiPlatform.Routing.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Routing.Application.Selection;

public sealed class UpsertRoutingConfigurationHandler(
    IRoutingConfigurationRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<UpsertRoutingConfigurationCommand, RoutingConfigurationResponse>
{
    public async Task<Result<RoutingConfigurationResponse>> Handle(
        UpsertRoutingConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var existing = await repository.GetAsync(tenantId, cancellationToken);
        var configuration = RoutingMappings.Map(command.Request, tenantId, existing?.Id ?? RoutingConfigurationId.New(), existing?.CreatedAtUtc ?? DateTimeOffset.UtcNow);
        if (existing is null)
        {
            await repository.UpsertAsync(configuration, cancellationToken);
            return Result.Success(RoutingMappings.Map(configuration));
        }

        existing.Update(
            configuration.Name,
            configuration.Mode,
            configuration.Enabled,
            configuration.Models,
            configuration.Rules,
            configuration.Departments,
            configuration.Repositories,
            configuration.Weights,
            configuration.Defaults,
            configuration.Metadata,
            DateTimeOffset.UtcNow);

        await repository.UpsertAsync(existing, cancellationToken);
        return Result.Success(RoutingMappings.Map(existing));
    }
}

public sealed class GetRoutingConfigurationHandler(
    IRoutingConfigurationRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetRoutingConfigurationQuery, RoutingConfigurationResponse>
{
    public async Task<Result<RoutingConfigurationResponse>> Handle(
        GetRoutingConfigurationQuery query,
        CancellationToken cancellationToken)
    {
        var configuration = await repository.GetAsync(requestContext.Current.TenantId, cancellationToken);
        return configuration is null
            ? Result.Failure<RoutingConfigurationResponse>(RoutingErrors.NotFound())
            : Result.Success(RoutingMappings.Map(configuration));
    }
}

public sealed class DeleteRoutingConfigurationHandler(
    IRoutingConfigurationRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<DeleteRoutingConfigurationCommand>
{
    public async Task<Result> Handle(DeleteRoutingConfigurationCommand command, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(requestContext.Current.TenantId, cancellationToken);
        return Result.Success();
    }
}

public sealed class EvaluateRouteHandler(
    IRoutingConfigurationRepository repository,
    IRoutingEvaluator evaluator,
    IRoutingTelemetry telemetry,
    IRequestContextAccessor requestContext)
    : ICommandHandler<EvaluateRouteCommand, RouteEvaluationResponse>
{
    public async Task<Result<RouteEvaluationResponse>> Handle(
        EvaluateRouteCommand command,
        CancellationToken cancellationToken)
    {
        var configuration = await repository.GetAsync(requestContext.Current.TenantId, cancellationToken);
        if (configuration is null)
        {
            return Result.Failure<RouteEvaluationResponse>(RoutingErrors.ConfigurationMissing());
        }

        var started = DateTimeOffset.UtcNow;
        var evaluation = evaluator.Evaluate(configuration, command.Request);
        telemetry.RecordEvaluation(evaluation.Mode, evaluation.Category, evaluation.SelectedModelKey, DateTimeOffset.UtcNow - started);
        return Result.Success(RoutingMappings.Map(evaluation));
    }
}

internal static class RoutingMappings
{
    internal static RoutingConfiguration Map(RoutingConfigurationRequest request, TenantId tenantId, RoutingConfigurationId id, DateTimeOffset createdAtUtc)
    {
        return new RoutingConfiguration(
            id,
            tenantId,
            request.Name,
            request.Mode,
            request.Enabled,
            request.Models.Select(Map).ToList(),
            request.Rules.Select(Map).ToList(),
            request.Departments?.ToDictionary(item => item.Key, item => Map(item.Value), StringComparer.OrdinalIgnoreCase),
            request.Repositories?.ToDictionary(item => item.Key, item => Map(item.Value), StringComparer.OrdinalIgnoreCase),
            new RoutingScoringWeights(
                request.Weights.CapabilityWeight,
                request.Weights.CostWeight,
                request.Weights.LatencyWeight,
                request.Weights.AvailabilityWeight,
                request.Weights.HealthWeight,
                request.Weights.ContextHeadroomWeight,
                request.Weights.BudgetWeight,
                request.Weights.RuleWeight),
            new RoutingDefaults(
                request.Defaults.FallbackModelKey,
                request.Defaults.DefaultMode,
                request.Defaults.MinimumAvailabilityPercent,
                request.Defaults.MaxCandidates,
                request.Defaults.MaxAlternates),
            request.Metadata,
            createdAtUtc);
    }

    internal static RoutingConfigurationResponse Map(RoutingConfiguration configuration)
    {
        return new RoutingConfigurationResponse(
            configuration.Id.Value,
            configuration.TenantId.Value,
            configuration.Name,
            configuration.Mode,
            configuration.Enabled,
            configuration.Models.Select(Map).ToList(),
            configuration.Rules.Select(Map).ToList(),
            configuration.Departments.ToDictionary(item => item.Key, item => Map(item.Value), StringComparer.OrdinalIgnoreCase),
            configuration.Repositories.ToDictionary(item => item.Key, item => Map(item.Value), StringComparer.OrdinalIgnoreCase),
            new RoutingScoringWeightsResponse(
                configuration.Weights.CapabilityWeight,
                configuration.Weights.CostWeight,
                configuration.Weights.LatencyWeight,
                configuration.Weights.AvailabilityWeight,
                configuration.Weights.HealthWeight,
                configuration.Weights.ContextHeadroomWeight,
                configuration.Weights.BudgetWeight,
                configuration.Weights.RuleWeight),
            new RoutingDefaultsResponse(
                configuration.Defaults.FallbackModelKey,
                configuration.Defaults.DefaultMode,
                configuration.Defaults.MinimumAvailabilityPercent,
                configuration.Defaults.MaxCandidates,
                configuration.Defaults.MaxAlternates),
            new Dictionary<string, string>(configuration.Metadata, StringComparer.OrdinalIgnoreCase),
            configuration.Version,
            configuration.CreatedAtUtc,
            configuration.UpdatedAtUtc);
    }

    internal static RoutingModelProfile Map(RoutingModelProfileRequest request)
        => new(
            request.ModelKey,
            request.Provider,
            request.ProviderModelName,
            request.DisplayName,
            request.Capabilities,
            request.InputTokenCostPer1K,
            request.OutputTokenCostPer1K,
            request.Currency,
            request.P50Ms,
            request.P95Ms,
            request.P99Ms,
            request.ContextSize,
            request.AvailabilityPercent,
            request.Health,
            request.Enabled,
            request.Metadata);

    internal static RoutingModelProfileResponse Map(RoutingModelProfile model)
        => new(
            model.ModelKey,
            model.Provider,
            model.ProviderModelName,
            model.DisplayName,
            model.Capabilities.ToList(),
            model.InputTokenCostPer1K,
            model.OutputTokenCostPer1K,
            model.Currency,
            model.P50Ms,
            model.P95Ms,
            model.P99Ms,
            model.ContextSize,
            model.AvailabilityPercent,
            model.Health,
            model.Enabled,
            new Dictionary<string, string>(model.Metadata, StringComparer.OrdinalIgnoreCase));

    internal static RoutingRule Map(RoutingRuleRequest request)
        => new(
            request.Name,
            request.Priority,
            request.Enabled,
            request.RequiredDepartments,
            request.RequiredRepositories,
            request.IncludeKeywords,
            request.ExcludeKeywords,
            request.RequiredCapabilities,
            request.MinimumComplexity,
            request.MaximumEstimatedTokens,
            request.MaximumEstimatedCostUsd,
            request.ModeOverride,
            request.SelectedModelKey);

    internal static RoutingRuleResponse Map(RoutingRule rule)
        => new(
            rule.Name,
            rule.Priority,
            rule.Enabled,
            rule.RequiredDepartments.ToList(),
            rule.RequiredRepositories.ToList(),
            rule.IncludeKeywords.ToList(),
            rule.ExcludeKeywords.ToList(),
            rule.RequiredCapabilities.ToList(),
            rule.MinimumComplexity,
            rule.MaximumEstimatedTokens,
            rule.MaximumEstimatedCostUsd,
            rule.ModeOverride,
            rule.SelectedModelKey);

    internal static RoutingScopeProfile Map(RoutingScopeProfileRequest request)
        => new(
            request.Name,
            request.PreferredModelKey,
            request.AllowedModelKeys,
            request.AllowedProviders,
            request.MaximumEstimatedCostUsd,
            request.MaximumEstimatedTokens,
            request.MonthlyBudgetUsd,
            request.MonthlyTokenBudget,
            request.Enabled,
            request.Priority,
            request.Notes);

    internal static RoutingScopeProfileResponse Map(RoutingScopeProfile profile)
        => new(
            profile.Name,
            profile.PreferredModelKey,
            profile.AllowedModelKeys.ToList(),
            profile.AllowedProviders.ToList(),
            profile.MaximumEstimatedCostUsd,
            profile.MaximumEstimatedTokens,
            profile.MonthlyBudgetUsd,
            profile.MonthlyTokenBudget,
            profile.Enabled,
            profile.Priority,
            profile.Notes);

    internal static RouteEvaluationResponse Map(RoutingEvaluationResult result)
        => new(
            result.Mode,
            result.Category,
            result.Complexity,
            result.ComplexityScore,
            result.EstimatedInputTokens,
            result.EstimatedOutputTokens,
            result.EstimatedCostUsd,
            result.SelectedModelKey,
            result.SelectedProvider,
            result.SelectedProviderModelName,
            result.SelectedDisplayName,
            result.Reason,
            result.MatchedRule,
            result.Department,
            result.Repository,
            result.BudgetExceeded,
            result.BudgetHeadroomUsd,
            result.Alternates.Select(option => new RoutingModelOptionResponse(
                option.ModelKey,
                option.Provider,
                option.ProviderModelName,
                option.DisplayName,
                option.Score,
                option.EstimatedCostUsd,
                option.EstimatedTokens,
                option.EstimatedLatencyMs,
                option.AvailabilityPercent)).ToList(),
            result.EvaluatedAtUtc);
}

using EnterpriseAiPlatform.Routing.Application.Selection;
using EnterpriseAiPlatform.Routing.Contracts.Requests;
using FluentValidation;

namespace EnterpriseAiPlatform.Routing.Application.Validation;

public sealed class RoutingConfigurationRequestValidator : AbstractValidator<RoutingConfigurationRequest>
{
    public RoutingConfigurationRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(120);
        RuleFor(request => request.Models).NotNull();
        RuleFor(request => request.Rules).NotNull();
        RuleFor(request => request.Weights.CapabilityWeight).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Weights.CostWeight).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Weights.LatencyWeight).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Weights.AvailabilityWeight).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Weights.HealthWeight).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Weights.ContextHeadroomWeight).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Weights.BudgetWeight).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Weights.RuleWeight).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Defaults.MinimumAvailabilityPercent).InclusiveBetween(0, 100);
        RuleFor(request => request.Defaults.MaxCandidates).InclusiveBetween(1, 100);
        RuleFor(request => request.Defaults.MaxAlternates).InclusiveBetween(1, 10);
    }
}

public sealed class RouteEvaluationRequestValidator : AbstractValidator<RouteEvaluationRequest>
{
    public RouteEvaluationRequestValidator()
    {
        RuleFor(request => request.Prompt).NotEmpty().MaximumLength(200_000);
        RuleFor(request => request.SystemPrompt).MaximumLength(50_000).When(request => request.SystemPrompt is not null);
        RuleFor(request => request.Department).MaximumLength(120).When(request => request.Department is not null);
        RuleFor(request => request.Repository).MaximumLength(200).When(request => request.Repository is not null);
        RuleFor(request => request.TaskType).MaximumLength(120).When(request => request.TaskType is not null);
        RuleFor(request => request.MaxTokens).GreaterThan(0).When(request => request.MaxTokens.HasValue);
        RuleFor(request => request.MaxCostUsd).GreaterThanOrEqualTo(0).When(request => request.MaxCostUsd.HasValue);
    }
}

public sealed class RoutingModeQueryValidator : AbstractValidator<RoutingModeQuery>
{
    public RoutingModeQueryValidator()
    {
        RuleFor(query => query.Take).InclusiveBetween(1, 100);
    }
}

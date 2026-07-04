using EnterpriseAiPlatform.ModelRegistry.Application.Search;
using EnterpriseAiPlatform.ModelRegistry.Contracts.Requests;
using FluentValidation;

namespace EnterpriseAiPlatform.ModelRegistry.Application.Validation;

public sealed class ModelRegistryWriteRequestValidator : AbstractValidator<ModelRegistryWriteRequest>
{
    public ModelRegistryWriteRequestValidator()
    {
        RuleFor(request => request.ProviderModelName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(4000).When(request => request.Description is not null);
        RuleFor(request => request.ContextSize).GreaterThan(0).LessThanOrEqualTo(2_000_000);
        RuleFor(request => request.Capabilities).NotNull();
        RuleFor(request => request.Pricing.Currency).NotEmpty().MaximumLength(10);
        RuleFor(request => request.Pricing.InputTokenCostPer1K).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Pricing.OutputTokenCostPer1K).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Pricing.CachedInputTokenCostPer1K).GreaterThanOrEqualTo(0).When(request => request.Pricing.CachedInputTokenCostPer1K.HasValue);
        RuleFor(request => request.Latency.P50Ms).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Latency.P95Ms).GreaterThanOrEqualTo(request => request.Latency.P50Ms);
        RuleFor(request => request.Latency.P99Ms).GreaterThanOrEqualTo(request => request.Latency.P95Ms);
        RuleFor(request => request.Availability.AvailabilityPercent).InclusiveBetween(0, 100);
        RuleFor(request => request.Configuration).Must(configuration => configuration is null || configuration.Count <= 100);
    }
}

public sealed class ListModelRegistryQueryValidator : AbstractValidator<ListModelRegistryQuery>
{
    public ListModelRegistryQueryValidator()
    {
        RuleFor(query => query.Skip).GreaterThanOrEqualTo(0);
        RuleFor(query => query.Take).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(200).When(query => query.Search is not null);
    }
}

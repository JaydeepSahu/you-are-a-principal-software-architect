using EnterpriseAiPlatform.PromptIntelligence.Application.Optimization;
using EnterpriseAiPlatform.PromptIntelligence.Contracts.Requests;
using FluentValidation;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Validation;

public sealed class CreateProfileValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Request.DefaultRule)
            .NotNull().WithMessage("Default rule is required.");

        When(x => x.Request.DefaultRule is not null, () =>
        {
            RuleFor(x => x.Request.DefaultRule!.MaxTokensTarget)
                .GreaterThan(0).WithMessage("MaxTokensTarget must be positive.")
                .When(x => x.Request.DefaultRule!.MaxTokensTarget.HasValue);

            RuleFor(x => x.Request.DefaultRule!.MinTokenReductionPercent)
                .InclusiveBetween(0, 100).WithMessage("MinTokenReductionPercent must be between 0 and 100.")
                .When(x => x.Request.DefaultRule!.MinTokenReductionPercent.HasValue);
        });
    }
}

public sealed class OptimizePromptValidator : AbstractValidator<OptimizePromptCommand>
{
    public OptimizePromptValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MaximumLength(100_000).WithMessage("Prompt must not exceed 100,000 characters.");
    }
}

public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}

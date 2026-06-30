using FluentValidation;

namespace EnterpriseAiPlatform.Identity.Application.Tokens;

public sealed class RefreshAccessTokenCommandValidator : AbstractValidator<RefreshAccessTokenCommand>
{
    public RefreshAccessTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(command => command.CorrelationId)
            .NotEmpty()
            .MaximumLength(128);
    }
}

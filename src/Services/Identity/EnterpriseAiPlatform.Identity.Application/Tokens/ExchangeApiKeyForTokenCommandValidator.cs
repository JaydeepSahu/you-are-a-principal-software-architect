using FluentValidation;

namespace EnterpriseAiPlatform.Identity.Application.Tokens;

public sealed class ExchangeApiKeyForTokenCommandValidator : AbstractValidator<ExchangeApiKeyForTokenCommand>
{
    public ExchangeApiKeyForTokenCommandValidator()
    {
        RuleFor(command => command.ApiKey)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(command => command.CorrelationId)
            .NotEmpty()
            .MaximumLength(128);
    }
}

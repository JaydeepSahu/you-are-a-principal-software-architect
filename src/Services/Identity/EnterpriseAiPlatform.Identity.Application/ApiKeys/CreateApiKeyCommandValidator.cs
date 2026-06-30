using EnterpriseAiPlatform.Identity.Contracts;
using FluentValidation;

namespace EnterpriseAiPlatform.Identity.Application.ApiKeys;

public sealed class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.TenantId.Value)
            .NotEmpty();

        RuleFor(command => command.ApplicationId)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(command => command.Roles)
            .NotEmpty()
            .Must(roles => roles.All(IdentityRoles.All.Contains))
            .WithMessage("One or more requested roles are not valid.");

        RuleFor(command => command.ExpiresAtUtc)
            .GreaterThan(_ => timeProvider.GetUtcNow())
            .LessThanOrEqualTo(_ => timeProvider.GetUtcNow().AddDays(366));

        RuleFor(command => command.CorrelationId)
            .NotEmpty()
            .MaximumLength(128);
    }
}

using EnterpriseAiPlatform.Evaluation.Application.Evaluations;
using EnterpriseAiPlatform.SharedKernel;
using EvaluationDomain = EnterpriseAiPlatform.Evaluation.Domain;
using EvaluationErrors = EnterpriseAiPlatform.Evaluation.Application.EvaluationErrors;
using FluentValidation;
using MediatR;

namespace EnterpriseAiPlatform.Evaluation.Application.Validation;

public sealed class CreateEvaluationValidator : AbstractValidator<CreateEvaluationCommand>
{
    public CreateEvaluationValidator()
    {
        RuleFor(x => x.Request.TargetId).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Request.TargetType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.LatencyMs).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.AcceptanceRate).InclusiveBetween(0, 1);
        RuleFor(x => x.Request.Confidence).InclusiveBetween(0, 1);
        RuleFor(x => x.Request.Similarity).InclusiveBetween(0, 1);
        RuleFor(x => x.Request.LintScore).InclusiveBetween(0, 1).When(x => x.Request.LintScore.HasValue);
        RuleFor(x => x.Request.HallucinationScore).InclusiveBetween(0, 1).When(x => x.Request.HallucinationScore.HasValue);
        RuleFor(x => x.Request.CostUsd).GreaterThanOrEqualTo(0).When(x => x.Request.CostUsd.HasValue);
    }
}

public sealed class ListEvaluationsValidator : AbstractValidator<ListEvaluationsQuery>
{
    public ListEvaluationsValidator()
    {
        RuleFor(x => x.Request.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Request.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        var error = EvaluationErrors.ValidationFailed(string.Join("; ", failures.Select(f => f.ErrorMessage)));
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var valueType = responseType.GetGenericArguments()[0];
        var method = typeof(Result).GetMethod(nameof(Result.Failure), 1, [typeof(ErrorDetail)])!
            .MakeGenericMethod(valueType);
        return (TResponse)method.Invoke(null, [error])!;
    }
}

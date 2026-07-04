using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using FluentValidation;
using MediatR;

namespace EnterpriseAiPlatform.Knowledge.Application.Validation;

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
            .Select(validator => validator.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var message = string.Join("; ", failures.Select(failure => failure.ErrorMessage));
        var error = KnowledgeErrors.ValidationFailed(message);
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var valueType = responseType.GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethods()
            .Single(method => method.Name == nameof(Result.Failure)
                && method.IsGenericMethodDefinition
                && method.GetParameters().Length == 1)
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}

using EnterpriseAiPlatform.SharedKernel;
using FluentValidation;
using MediatR;

namespace EnterpriseAiPlatform.VectorSearch.Application.Validation;

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
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        var error = VectorSearchErrors.ValidationFailed(string.Join("; ", failures.Select(failure => failure.ErrorMessage)));
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

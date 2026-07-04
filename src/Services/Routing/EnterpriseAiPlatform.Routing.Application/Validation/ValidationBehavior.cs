using EnterpriseAiPlatform.Routing.Application;
using EnterpriseAiPlatform.SharedKernel;
using FluentValidation;
using MediatR;

namespace EnterpriseAiPlatform.Routing.Application.Validation;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        var error = RoutingErrors.ValidationFailed(string.Join("; ", failures.Select(failure => failure.ErrorMessage)));
        var responseType = typeof(TResponse);
        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var genericArgument = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure), 1, [typeof(ErrorDetail)])!;
            var typedFailure = failureMethod.MakeGenericMethod(genericArgument).Invoke(null, [error])!;
            return (TResponse)typedFailure;
        }

        throw new InvalidOperationException($"Validation behavior cannot handle response type '{responseType.Name}'.");
    }
}

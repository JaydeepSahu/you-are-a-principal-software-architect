using EnterpriseAiPlatform.SharedKernel;
using FluentValidation;
using MediatR;

namespace EnterpriseAiPlatform.Identity.Application.Validation;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (!_validators.Any())
        {
            return await next();
        }

        ValidationContext<TRequest> context = new(request);
        FluentValidation.Results.ValidationFailure[] failures = _validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        string validationMessage = string.Join("; ", failures.Select(failure => failure.ErrorMessage));
        object failureResult = CreateFailureResult(typeof(TResponse), validationMessage);
        return (TResponse)failureResult;
    }

    private static object CreateFailureResult(Type responseType, string message)
    {
        ErrorDetail error = ErrorDetail.Create("identity.validation_failed", message);

        if (responseType == typeof(Result))
        {
            return Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type valueType = responseType.GetGenericArguments()[0];
            return typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, [typeof(ErrorDetail)])!
                .MakeGenericMethod(valueType)
                .Invoke(null, [error])!;
        }

        throw new InvalidOperationException("Validation behavior can only create Result response types.");
    }
}

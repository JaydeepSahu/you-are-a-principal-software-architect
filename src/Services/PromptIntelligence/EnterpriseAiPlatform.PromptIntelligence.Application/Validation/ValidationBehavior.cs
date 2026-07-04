using EnterpriseAiPlatform.SharedKernel;
using FluentValidation;
using MediatR;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Validation;

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
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        string message = string.Join("; ", failures.Select(f => f.ErrorMessage));
        return CreateFailureResult(typeof(TResponse), message);
    }

    private static TResponse CreateFailureResult(Type responseType, string message)
    {
        var error = ErrorDetail.Create("prompt_intelligence.validation_failed", message);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var method = typeof(Result).GetMethod(nameof(Result.Failure), 1, [typeof(ErrorDetail)])!
                .MakeGenericMethod(valueType);
            return (TResponse)method.Invoke(null, [error])!;
        }

        throw new InvalidOperationException("ValidationBehavior only supports Result response types.");
    }
}

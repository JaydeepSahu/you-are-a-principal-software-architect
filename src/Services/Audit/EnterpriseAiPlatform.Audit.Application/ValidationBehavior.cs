using EnterpriseAiPlatform.Audit.Application.Validation;
using FluentValidation;
using MediatR;

namespace EnterpriseAiPlatform.Audit.Application;

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
            return await next();

        var error = AuditErrors.ValidationFailed(string.Join("; ", failures.Select(f => f.ErrorMessage)));
        var responseType = typeof(TResponse);
        if (responseType == typeof(SharedKernel.Result))
            return (TResponse)(object)SharedKernel.Result.Failure(error);

        var valueType = responseType.GetGenericArguments()[0];
        var method = typeof(SharedKernel.Result).GetMethod(nameof(SharedKernel.Result.Failure), 1, [typeof(SharedKernel.ErrorDetail)])!
            .MakeGenericMethod(valueType);
        return (TResponse)method.Invoke(null, [error])!;
    }
}

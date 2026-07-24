using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Infrastructure.Execution;

public static class RetryPolicyExecutor
{
    public static async Task<Result<T>> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        int maxRetries,
        Action<int, Exception>? onRetry = null,
        CancellationToken cancellationToken = default)
    {
        int attempt = 0;
        int delayMs = 100;

        while (true)
        {
            attempt++;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await action(cancellationToken);
                if (result.IsSuccess || attempt >= maxRetries)
                {
                    return result;
                }
            }
            catch (Exception ex) when (attempt < maxRetries && ex is not OperationCanceledException)
            {
                onRetry?.Invoke(attempt, ex);
            }

            if (attempt >= maxRetries)
            {
                return Result.Failure<T>(new Error("Retry.Exhausted", $"Failed after {maxRetries} attempt(s)."));
            }

            await Task.Delay(delayMs, cancellationToken);
            delayMs *= 2; // Exponential backoff
        }
    }
}

using EnterpriseAiPlatform.ProviderAdapters.Application.Abstractions;
using EnterpriseAiPlatform.ProviderAdapters.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ProviderAdapters.Application;

public sealed class ProviderOrchestrator(
    IProviderPluginRegistry registry,
    IProviderCircuitBreakerStore circuitBreakerStore,
    IProviderTelemetry telemetry,
    IServiceProvider serviceProvider)
    : IProviderOrchestrator
{
    public async Task<Result<ProviderInvocationResponse>> InvokeAsync(
        ProviderInvocationRequest request,
        ProviderInvocationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var candidates = ResolveCandidates(request);
        foreach (var candidate in candidates)
        {
            if (circuitBreakerStore.IsOpen(candidate.Descriptor.ProviderKey, out var retryAfter))
            {
                telemetry.RecordCircuitState(candidate.Descriptor.ProviderKey, candidate.Descriptor.ProviderKind, ProviderHealthState.CircuitOpen, $"retry-after:{retryAfter}");
                continue;
            }

            var result = await ExecuteInvocationWithRetryAsync(candidate, request, context, cancellationToken);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        var fallbackKey = request.FallbackProviderKeys.Count > 0 ? request.FallbackProviderKeys[0] : null;
        return Result.Failure<ProviderInvocationResponse>(
            ProviderAdaptersErrors.NoProviderRegistered(request.PreferredProviderKey ?? fallbackKey));
    }

    public async Task<Result<ProviderStreamingSession>> StreamAsync(
        ProviderInvocationRequest request,
        ProviderInvocationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var streamingRequest = new ProviderInvocationRequest(
            request.Messages,
            request.Model,
            true,
            request.MaxOutputTokens,
            request.Temperature,
            request.TopP,
            request.PreferredProviderKey,
            request.FallbackProviderKeys,
            request.Timeout,
            request.MaxRetries,
            request.Metadata);
        var candidates = ResolveCandidates(streamingRequest);
        foreach (var candidate in candidates)
        {
            if (circuitBreakerStore.IsOpen(candidate.Descriptor.ProviderKey, out var retryAfter))
            {
                telemetry.RecordCircuitState(candidate.Descriptor.ProviderKey, candidate.Descriptor.ProviderKind, ProviderHealthState.CircuitOpen, $"retry-after:{retryAfter}");
                continue;
            }

            var result = await ExecuteStreamingWithRetryAsync(candidate, streamingRequest, context, cancellationToken);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        var fallbackKey = streamingRequest.FallbackProviderKeys.Count > 0 ? streamingRequest.FallbackProviderKeys[0] : null;
        return Result.Failure<ProviderStreamingSession>(
            ProviderAdaptersErrors.NoProviderRegistered(streamingRequest.PreferredProviderKey ?? fallbackKey));
    }

    private IEnumerable<IProviderPlugin> ResolveCandidates(ProviderInvocationRequest request)
    {
        var orderedKeys = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.PreferredProviderKey))
        {
            orderedKeys.Add(request.PreferredProviderKey);
        }

        orderedKeys.AddRange(request.FallbackProviderKeys);

        if (orderedKeys.Count == 0)
        {
            orderedKeys.AddRange(registry.GetAll().Select(plugin => plugin.Descriptor.ProviderKey));
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in orderedKeys)
        {
            if (!seen.Add(key))
            {
                continue;
            }

            if (registry.TryGet(key, out var plugin))
            {
                yield return plugin;
            }
        }
    }

    private async Task<Result<ProviderInvocationResponse>> ExecuteInvocationWithRetryAsync(
        IProviderPlugin plugin,
        ProviderInvocationRequest request,
        ProviderInvocationContext context,
        CancellationToken cancellationToken)
    {
        var adapter = plugin.CreateAdapter(serviceProvider);
        var policy = plugin.Descriptor.ResiliencePolicy;
        var retryCount = request.MaxRetries ?? policy.MaxRetries;
        var timeout = request.Timeout ?? policy.Timeout;
        var attempt = 0;
        var delay = policy.InitialRetryDelay;

        while (true)
        {
            var started = DateTimeOffset.UtcNow;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            try
            {
                var result = await adapter.InvokeAsync(request, context, timeoutCts.Token);

                telemetry.RecordAttempt(plugin.Descriptor.ProviderKey, plugin.Descriptor.ProviderKind, false, DateTimeOffset.UtcNow - started, result.IsSuccess, result.IsSuccess ? null : result.Error.Message);
                if (result.IsSuccess)
                {
                    circuitBreakerStore.RecordSuccess(plugin.Descriptor.ProviderKey);
                    telemetry.RecordCircuitState(plugin.Descriptor.ProviderKey, plugin.Descriptor.ProviderKind, circuitBreakerStore.GetHealthState(plugin.Descriptor.ProviderKey));
                    return result;
                }

                circuitBreakerStore.RecordFailure(plugin.Descriptor.ProviderKey, plugin.Descriptor.ResiliencePolicy.CircuitBreakerOpenDuration, plugin.Descriptor.ResiliencePolicy.CircuitBreakerFailureThreshold);
                if (attempt++ >= retryCount)
                {
                    return result;
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                telemetry.RecordAttempt(plugin.Descriptor.ProviderKey, plugin.Descriptor.ProviderKind, false, DateTimeOffset.UtcNow - started, false, "timeout");
                circuitBreakerStore.RecordFailure(plugin.Descriptor.ProviderKey, plugin.Descriptor.ResiliencePolicy.CircuitBreakerOpenDuration, plugin.Descriptor.ResiliencePolicy.CircuitBreakerFailureThreshold);
                if (attempt++ >= retryCount)
                {
                    return Result.Failure<ProviderInvocationResponse>(
                        ProviderAdaptersErrors.ProviderInvocationFailed(plugin.Descriptor.ProviderKey, "timeout"));
                }
            }
            catch (Exception ex)
            {
                telemetry.RecordAttempt(plugin.Descriptor.ProviderKey, plugin.Descriptor.ProviderKind, false, DateTimeOffset.UtcNow - started, false, ex.Message);
                circuitBreakerStore.RecordFailure(plugin.Descriptor.ProviderKey, plugin.Descriptor.ResiliencePolicy.CircuitBreakerOpenDuration, plugin.Descriptor.ResiliencePolicy.CircuitBreakerFailureThreshold);
                if (attempt++ >= retryCount)
                {
                    return Result.Failure<ProviderInvocationResponse>(
                        ProviderAdaptersErrors.ProviderInvocationFailed(plugin.Descriptor.ProviderKey, ex.Message));
                }
            }

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Max(1.0d, policy.BackoffMultiplier));
            }
        }
    }

    private async Task<Result<ProviderStreamingSession>> ExecuteStreamingWithRetryAsync(
        IProviderPlugin plugin,
        ProviderInvocationRequest request,
        ProviderInvocationContext context,
        CancellationToken cancellationToken)
    {
        var adapter = plugin.CreateAdapter(serviceProvider);
        var policy = plugin.Descriptor.ResiliencePolicy;
        var retryCount = request.MaxRetries ?? policy.MaxRetries;
        var timeout = request.Timeout ?? policy.Timeout;
        var attempt = 0;
        var delay = policy.InitialRetryDelay;

        while (true)
        {
            var started = DateTimeOffset.UtcNow;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            try
            {
                var result = await adapter.StreamAsync(request, context, timeoutCts.Token);
                telemetry.RecordAttempt(plugin.Descriptor.ProviderKey, plugin.Descriptor.ProviderKind, true, DateTimeOffset.UtcNow - started, result.IsSuccess, result.IsSuccess ? null : result.Error.Message);
                if (result.IsSuccess)
                {
                    circuitBreakerStore.RecordSuccess(plugin.Descriptor.ProviderKey);
                    telemetry.RecordCircuitState(plugin.Descriptor.ProviderKey, plugin.Descriptor.ProviderKind, circuitBreakerStore.GetHealthState(plugin.Descriptor.ProviderKey));
                    return result;
                }

                circuitBreakerStore.RecordFailure(plugin.Descriptor.ProviderKey, plugin.Descriptor.ResiliencePolicy.CircuitBreakerOpenDuration, plugin.Descriptor.ResiliencePolicy.CircuitBreakerFailureThreshold);
                if (attempt++ >= retryCount)
                {
                    return result;
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                telemetry.RecordAttempt(plugin.Descriptor.ProviderKey, plugin.Descriptor.ProviderKind, true, DateTimeOffset.UtcNow - started, false, "timeout");
                circuitBreakerStore.RecordFailure(plugin.Descriptor.ProviderKey, plugin.Descriptor.ResiliencePolicy.CircuitBreakerOpenDuration, plugin.Descriptor.ResiliencePolicy.CircuitBreakerFailureThreshold);
                if (attempt++ >= retryCount)
                {
                    return Result.Failure<ProviderStreamingSession>(
                        ProviderAdaptersErrors.ProviderInvocationFailed(plugin.Descriptor.ProviderKey, "timeout"));
                }
            }
            catch (Exception ex)
            {
                telemetry.RecordAttempt(plugin.Descriptor.ProviderKey, plugin.Descriptor.ProviderKind, true, DateTimeOffset.UtcNow - started, false, ex.Message);
                circuitBreakerStore.RecordFailure(plugin.Descriptor.ProviderKey, plugin.Descriptor.ResiliencePolicy.CircuitBreakerOpenDuration, plugin.Descriptor.ResiliencePolicy.CircuitBreakerFailureThreshold);
                if (attempt++ >= retryCount)
                {
                    return Result.Failure<ProviderStreamingSession>(
                        ProviderAdaptersErrors.ProviderInvocationFailed(plugin.Descriptor.ProviderKey, ex.Message));
                }
            }

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Max(1.0d, policy.BackoffMultiplier));
            }
        }
    }
}

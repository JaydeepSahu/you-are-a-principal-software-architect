using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.LocalModel.Application;

public sealed class LocalModelOrchestrator(
    ILocalModelPluginRegistry registry,
    ILocalModelHealthStore healthStore,
    ILocalModelLoadBalancer loadBalancer,
    IServiceProvider serviceProvider)
    : ILocalModelOrchestrator
{
    public async Task<Result<LocalModelChatResponse>> ChatAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var candidates = ResolveCandidates(request);

        foreach (var candidate in candidates)
        {
            if (!healthStore.TryAcquire(candidate.Descriptor.ProviderKey, candidate.Descriptor.MaxConcurrency, out _))
            {
                continue;
            }

            try
            {
                var provider = candidate.CreateProvider(serviceProvider);
                var result = await InvokeWithRetryAsync(provider, request, cancellationToken);
                if (result.IsSuccess)
                {
                    return result;
                }
            }
            finally
            {
                healthStore.Release(candidate.Descriptor.ProviderKey);
            }
        }

        return Result.Failure<LocalModelChatResponse>(LocalModelErrors.ProviderNotRegistered(request.PreferredProviderKey));
    }

    public async Task<Result<LocalModelStreamingSession>> StreamAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var streamingRequest = request with { Stream = true };
        var candidates = ResolveCandidates(streamingRequest);

        foreach (var candidate in candidates)
        {
            if (!healthStore.TryAcquire(candidate.Descriptor.ProviderKey, candidate.Descriptor.MaxConcurrency, out _))
            {
                continue;
            }

            try
            {
                var provider = candidate.CreateProvider(serviceProvider);
                var result = await InvokeStreamingWithRetryAsync(provider, streamingRequest, cancellationToken);
                if (result.IsSuccess)
                {
                    return result;
                }
            }
            finally
            {
                healthStore.Release(candidate.Descriptor.ProviderKey);
            }
        }

        return Result.Failure<LocalModelStreamingSession>(LocalModelErrors.ProviderNotRegistered(request.PreferredProviderKey));
    }

    public async Task<Result<IReadOnlyList<string>>> DiscoverModelsAsync(CancellationToken cancellationToken = default)
    {
        var models = new List<string>();
        foreach (var plugin in registry.GetAll())
        {
            var provider = plugin.CreateProvider(serviceProvider);
            var result = await provider.DiscoverModelsAsync(cancellationToken);
            if (result.IsSuccess)
            {
                models.AddRange(result.Value);
                continue;
            }

            return Result.Failure<IReadOnlyList<string>>(LocalModelErrors.DiscoveryFailed(plugin.Descriptor.ProviderKey, result.Error.Message));
        }

        return Result.Success<IReadOnlyList<string>>(models.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(model => model).ToArray());
    }

    public Task<Result<IReadOnlyList<LocalModelHealthSnapshot>>> GetHealthAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success<IReadOnlyList<LocalModelHealthSnapshot>>(healthStore.GetAllSnapshots().ToArray()));

    private IReadOnlyList<ILocalModelPlugin> ResolveCandidates(LocalModelChatRequest request)
    {
        var orderedKeys = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.PreferredProviderKey))
        {
            orderedKeys.Add(request.PreferredProviderKey);
        }

        if (request.FallbackProviderKeys is not null)
        {
            orderedKeys.AddRange(request.FallbackProviderKeys);
        }

        if (orderedKeys.Count == 0)
        {
            return registry.GetAll().OrderBy(plugin => plugin.Descriptor.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        var candidates = new List<ILocalModelPlugin>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in orderedKeys)
        {
            if (!seen.Add(key))
            {
                continue;
            }

            if (registry.TryGet(key, out var plugin))
            {
                candidates.Add(plugin);
            }
        }

        if (request.PreferredGpuIds is { Count: > 0 })
        {
            candidates = candidates
                .Where(plugin => plugin.Descriptor.Gpus.Count == 0 || plugin.Descriptor.Gpus.Any(gpu => request.PreferredGpuIds.Contains(gpu.GpuId, StringComparer.OrdinalIgnoreCase)))
                .ToList();
        }

        if (candidates.Count == 0)
        {
            return registry.GetAll().OrderBy(plugin => plugin.Descriptor.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        var selectedKey = loadBalancer.SelectProvider(candidates, request, healthStore);
        if (string.IsNullOrWhiteSpace(selectedKey))
        {
            return candidates;
        }

        return candidates
            .OrderByDescending(plugin => string.Equals(plugin.Descriptor.ProviderKey, selectedKey, StringComparison.OrdinalIgnoreCase))
            .ThenBy(plugin => plugin.Descriptor.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<Result<LocalModelChatResponse>> InvokeWithRetryAsync(
        ILocalModelProvider provider,
        LocalModelChatRequest request,
        CancellationToken cancellationToken)
    {
        var retryCount = request.MaxRetries ?? 0;
        var attempt = 0;
        var delay = TimeSpan.FromMilliseconds(100);

        while (true)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(request.Timeout ?? TimeSpan.FromSeconds(30));
            var started = DateTimeOffset.UtcNow;

            try
            {
                var result = await provider.ChatAsync(request, timeoutCts.Token);
                if (result.IsSuccess)
                {
                    healthStore.RecordHealth(new LocalModelHealthSnapshot(provider.Descriptor.ProviderKey, provider.Descriptor.Name, provider.Descriptor.BackendKind, LocalModelHealthState.Healthy, 0, provider.Descriptor.MaxConcurrency, DateTimeOffset.UtcNow));
                    return result;
                }

                healthStore.RecordHealth(new LocalModelHealthSnapshot(provider.Descriptor.ProviderKey, provider.Descriptor.Name, provider.Descriptor.BackendKind, LocalModelHealthState.Unhealthy, 0, provider.Descriptor.MaxConcurrency, DateTimeOffset.UtcNow, result.Error.Message));
                if (attempt++ >= retryCount)
                {
                    return result;
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                healthStore.RecordHealth(new LocalModelHealthSnapshot(provider.Descriptor.ProviderKey, provider.Descriptor.Name, provider.Descriptor.BackendKind, LocalModelHealthState.Degraded, 0, provider.Descriptor.MaxConcurrency, DateTimeOffset.UtcNow, "timeout"));
                if (attempt++ >= retryCount)
                {
                    return Result.Failure<LocalModelChatResponse>(LocalModelErrors.ProviderInvocationFailed(provider.Descriptor.ProviderKey, "timeout"));
                }
            }
            catch (Exception ex)
            {
                healthStore.RecordHealth(new LocalModelHealthSnapshot(provider.Descriptor.ProviderKey, provider.Descriptor.Name, provider.Descriptor.BackendKind, LocalModelHealthState.Unhealthy, 0, provider.Descriptor.MaxConcurrency, DateTimeOffset.UtcNow, ex.Message));
                if (attempt++ >= retryCount)
                {
                    return Result.Failure<LocalModelChatResponse>(LocalModelErrors.ProviderInvocationFailed(provider.Descriptor.ProviderKey, ex.Message));
                }
            }

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            delay = TimeSpan.FromMilliseconds(Math.Min(1000, delay.TotalMilliseconds * 2));
        }
    }

    private async Task<Result<LocalModelStreamingSession>> InvokeStreamingWithRetryAsync(
        ILocalModelProvider provider,
        LocalModelChatRequest request,
        CancellationToken cancellationToken)
    {
        var retryCount = request.MaxRetries ?? 0;
        var attempt = 0;
        var delay = TimeSpan.FromMilliseconds(100);

        while (true)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(request.Timeout ?? TimeSpan.FromSeconds(30));

            try
            {
                var result = await provider.StreamAsync(request, timeoutCts.Token);
                if (result.IsSuccess)
                {
                    healthStore.RecordHealth(new LocalModelHealthSnapshot(provider.Descriptor.ProviderKey, provider.Descriptor.Name, provider.Descriptor.BackendKind, LocalModelHealthState.Healthy, 0, provider.Descriptor.MaxConcurrency, DateTimeOffset.UtcNow));
                    return result;
                }

                healthStore.RecordHealth(new LocalModelHealthSnapshot(provider.Descriptor.ProviderKey, provider.Descriptor.Name, provider.Descriptor.BackendKind, LocalModelHealthState.Unhealthy, 0, provider.Descriptor.MaxConcurrency, DateTimeOffset.UtcNow, result.Error.Message));
                if (attempt++ >= retryCount)
                {
                    return result;
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                healthStore.RecordHealth(new LocalModelHealthSnapshot(provider.Descriptor.ProviderKey, provider.Descriptor.Name, provider.Descriptor.BackendKind, LocalModelHealthState.Degraded, 0, provider.Descriptor.MaxConcurrency, DateTimeOffset.UtcNow, "timeout"));
                if (attempt++ >= retryCount)
                {
                    return Result.Failure<LocalModelStreamingSession>(LocalModelErrors.ProviderInvocationFailed(provider.Descriptor.ProviderKey, "timeout"));
                }
            }
            catch (Exception ex)
            {
                healthStore.RecordHealth(new LocalModelHealthSnapshot(provider.Descriptor.ProviderKey, provider.Descriptor.Name, provider.Descriptor.BackendKind, LocalModelHealthState.Unhealthy, 0, provider.Descriptor.MaxConcurrency, DateTimeOffset.UtcNow, ex.Message));
                if (attempt++ >= retryCount)
                {
                    return Result.Failure<LocalModelStreamingSession>(LocalModelErrors.ProviderInvocationFailed(provider.Descriptor.ProviderKey, ex.Message));
                }
            }

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            delay = TimeSpan.FromMilliseconds(Math.Min(1000, delay.TotalMilliseconds * 2));
        }
    }
}

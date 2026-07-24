using System.Collections.Concurrent;
using System.Diagnostics;
using EnterpriseAiPlatform.AiGateway.Application.Abstractions;
using EnterpriseAiPlatform.Application.Abstractions.Configuration;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.AiGateway.Infrastructure.Resilience;

public sealed class ProviderCircuitBreakerManager : ICircuitBreakerManager
{
    private class CircuitTracker
    {
        public string ProviderName { get; }
        public string FallbackProvider { get; }
        public CircuitState State { get; set; } = CircuitState.Closed;
        public long SuccessCount;
        public long FailureCount;
        public DateTimeOffset LastStateChangeUtc { get; set; } = DateTimeOffset.UtcNow;
        public readonly object LockObj = new();

        public CircuitTracker(string providerName, string fallbackProvider)
        {
            ProviderName = providerName;
            FallbackProvider = fallbackProvider;
        }
    }

    private readonly ConcurrentDictionary<string, CircuitTracker> _trackers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ResilienceOptions _options;

    public ProviderCircuitBreakerManager(IOptions<ResilienceOptions> options)
    {
        _options = options?.Value ?? new ResilienceOptions();

        foreach (var (primary, fallback) in _options.ProviderFallbackChain)
        {
            _trackers[primary] = new CircuitTracker(primary, fallback);
        }
    }

    public ProviderCircuitStatus GetProviderStatus(string providerName)
    {
        var tracker = GetOrCreateTracker(providerName);
        EvaluateState(tracker);

        lock (tracker.LockObj)
        {
            long total = tracker.SuccessCount + tracker.FailureCount;
            double rate = total > 0 ? (double)tracker.FailureCount / total * 100.0 : 0.0;
            return new ProviderCircuitStatus(
                tracker.ProviderName,
                tracker.State,
                tracker.SuccessCount,
                tracker.FailureCount,
                Math.Round(rate, 1),
                tracker.LastStateChangeUtc,
                tracker.FallbackProvider
            );
        }
    }

    public IReadOnlyList<ProviderCircuitStatus> GetAllStatuses()
    {
        return _trackers.Keys.Select(GetProviderStatus).ToList().AsReadOnly();
    }

    public void RecordOutcome(string providerName, bool isSuccess, bool isRateLimit = false)
    {
        var tracker = GetOrCreateTracker(providerName);
        lock (tracker.LockObj)
        {
            if (isSuccess)
            {
                Interlocked.Increment(ref tracker.SuccessCount);
                if (tracker.State == CircuitState.HalfOpen)
                {
                    tracker.State = CircuitState.Closed;
                    tracker.LastStateChangeUtc = DateTimeOffset.UtcNow;
                    Interlocked.Exchange(ref tracker.FailureCount, 0);
                    Interlocked.Exchange(ref tracker.SuccessCount, 0);
                }
            }
            else
            {
                Interlocked.Increment(ref tracker.FailureCount);
                if (tracker.State == CircuitState.Closed)
                {
                    long total = tracker.SuccessCount + tracker.FailureCount;
                    if (total >= _options.MinimumThresholdCount)
                    {
                        double rate = (double)tracker.FailureCount / total * 100.0;
                        if (rate >= _options.FailureRateTripThresholdPercentage || isRateLimit)
                        {
                            tracker.State = CircuitState.Open;
                            tracker.LastStateChangeUtc = DateTimeOffset.UtcNow;
                        }
                    }
                }
            }
        }
    }

    public async Task<Result<ResilientRoutingResult>> RouteWithResilienceAsync(
        TenantId tenantId,
        ResilientRoutingRequest request,
        Func<string, CancellationToken, Task<Result<string>>> providerExecutor,
        CancellationToken cancellationToken = default)
    {
        var preferredTracker = GetOrCreateTracker(request.PreferredProvider);
        EvaluateState(preferredTracker);

        string targetProvider = request.PreferredProvider;
        bool wasFallback = false;

        if (preferredTracker.State == CircuitState.Open)
        {
            targetProvider = request.ExplicitFallbackProvider ?? preferredTracker.FallbackProvider;
            wasFallback = true;
        }

        var sw = Stopwatch.StartNew();
        var result = await providerExecutor(targetProvider, cancellationToken);
        sw.Stop();

        if (result.IsSuccess)
        {
            RecordOutcome(targetProvider, isSuccess: true);
            var status = GetProviderStatus(targetProvider);
            return Result<ResilientRoutingResult>.Success(new ResilientRoutingResult(
                targetProvider,
                wasFallback,
                result.Value,
                sw.ElapsedMilliseconds,
                status.State.ToString()
            ));
        }

        RecordOutcome(targetProvider, isSuccess: false, isRateLimit: result.Error.Code.Contains("RateLimit"));

        if (!wasFallback)
        {
            string fallbackTarget = request.ExplicitFallbackProvider ?? preferredTracker.FallbackProvider;
            sw.Restart();
            var fallbackResult = await providerExecutor(fallbackTarget, cancellationToken);
            sw.Stop();

            if (fallbackResult.IsSuccess)
            {
                RecordOutcome(fallbackTarget, isSuccess: true);
                var fallbackStatus = GetProviderStatus(fallbackTarget);
                return Result<ResilientRoutingResult>.Success(new ResilientRoutingResult(
                    fallbackTarget,
                    WasFallbackUsed: true,
                    fallbackResult.Value,
                    sw.ElapsedMilliseconds,
                    fallbackStatus.State.ToString()
                ));
            }
        }

        return Result<ResilientRoutingResult>.Failure(result.Error);
    }

    private CircuitTracker GetOrCreateTracker(string providerName)
    {
        return _trackers.GetOrAdd(providerName, name => new CircuitTracker(name, "SelfHostedVllm"));
    }

    private void EvaluateState(CircuitTracker tracker)
    {
        lock (tracker.LockObj)
        {
            if (tracker.State == CircuitState.Open && (DateTimeOffset.UtcNow - tracker.LastStateChangeUtc) > TimeSpan.FromSeconds(_options.OpenRecoveryTimeoutSeconds))
            {
                tracker.State = CircuitState.HalfOpen;
                tracker.LastStateChangeUtc = DateTimeOffset.UtcNow;
            }
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Net;
using EnterpriseAiPlatform.ProviderAdapters.Application.Abstractions;
using EnterpriseAiPlatform.ProviderAdapters.Domain;
using Microsoft.Extensions.Logging;

namespace EnterpriseAiPlatform.ProviderAdapters.Infrastructure.Resilience;

[SuppressMessage("Reliability", "CA1848:Use the LoggerMessage delegates", Justification = "Resilience pipeline logging.")]
public sealed class ResilientHttpClientHandler : DelegatingHandler
{
    private readonly IProviderCircuitBreakerStore _circuitBreakerStore;
    private readonly ProviderResiliencePolicy _policy;
    private readonly ILogger<ResilientHttpClientHandler> _logger;

    public ResilientHttpClientHandler(
        IProviderCircuitBreakerStore circuitBreakerStore,
        ProviderResiliencePolicy policy,
        ILogger<ResilientHttpClientHandler> logger)
    {
        _circuitBreakerStore = circuitBreakerStore;
        _policy = policy;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var providerKey = request.Headers.Contains("X-Provider-Key")
            ? request.Headers.GetValues("X-Provider-Key").FirstOrDefault() ?? "default"
            : "default";

        if (_circuitBreakerStore.IsOpen(providerKey, out var retryAfter))
        {
            _logger.LogWarning("Circuit breaker open for provider {ProviderKey}. Retry after {RetryAfter}.",
                providerKey, retryAfter);

            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                ReasonPhrase = $"Circuit breaker open for provider {providerKey}."
            };
        }

        int attempts = 0;
        TimeSpan delay = _policy.InitialRetryDelay;

        while (true)
        {
            attempts++;
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _circuitBreakerStore.RecordSuccess(providerKey);
                    return response;
                }

                if (IsTransientStatusCode(response.StatusCode) && attempts <= _policy.MaxRetries)
                {
                    _logger.LogWarning("Transient HTTP {StatusCode} from provider {ProviderKey} (Attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms.",
                        (int)response.StatusCode, providerKey, attempts, _policy.MaxRetries, delay.TotalMilliseconds);

                    await Task.Delay(delay, cancellationToken);
                    delay *= _policy.BackoffMultiplier;
                    continue;
                }

                _circuitBreakerStore.RecordFailure(providerKey, _policy.CircuitBreakerOpenDuration, _policy.CircuitBreakerFailureThreshold);
                return response;
            }
            catch (Exception ex) when (attempts <= _policy.MaxRetries && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Network error invoking provider {ProviderKey} (Attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms.",
                    providerKey, attempts, _policy.MaxRetries, delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
                delay *= _policy.BackoffMultiplier;
            }
        }
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.TooManyRequests
                     or HttpStatusCode.ServiceUnavailable
                     or HttpStatusCode.BadGateway
                     or HttpStatusCode.GatewayTimeout;
}

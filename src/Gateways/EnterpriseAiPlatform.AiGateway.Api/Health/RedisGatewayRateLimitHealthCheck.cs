using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace EnterpriseAiPlatform.AiGateway.Api.Health;

public sealed class RedisGatewayRateLimitHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisGatewayRateLimitHealthCheck(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_connectionMultiplexer.IsConnected)
        {
            return HealthCheckResult.Unhealthy("AI gateway Redis rate limiter is not connected.");
        }

        TimeSpan latency = await _connectionMultiplexer.GetDatabase().PingAsync();
        return HealthCheckResult.Healthy($"AI gateway Redis rate limiter responded in {latency.TotalMilliseconds:0.##} ms.");
    }
}

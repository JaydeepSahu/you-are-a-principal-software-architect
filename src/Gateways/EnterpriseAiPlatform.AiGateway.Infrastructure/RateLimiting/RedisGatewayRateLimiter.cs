using System.Globalization;
using EnterpriseAiPlatform.AiGateway.Application.RateLimiting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EnterpriseAiPlatform.AiGateway.Infrastructure.RateLimiting;

public sealed class RedisGatewayRateLimiter : IGatewayRateLimiter
{
    private const string IncrementScript = """
        local current = redis.call('INCR', KEYS[1])
        if current == 1 then
            redis.call('PEXPIRE', KEYS[1], ARGV[1])
        end
        return current
        """;

    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly GatewayRateLimitOptions _options;
    private readonly TimeProvider _timeProvider;

    public RedisGatewayRateLimiter(
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<GatewayRateLimitOptions> options,
        TimeProvider timeProvider)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public async ValueTask<GatewayRateLimitDecision> CheckAsync(
        GatewayRateLimitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset nowUtc = _timeProvider.GetUtcNow();
        long windowStart = nowUtc.ToUnixTimeSeconds() / _options.WindowSeconds * _options.WindowSeconds;
        DateTimeOffset resetAtUtc = DateTimeOffset.FromUnixTimeSeconds(windowStart + _options.WindowSeconds);
        IDatabase database = _connectionMultiplexer.GetDatabase();

        RedisResult result = await database.ScriptEvaluateAsync(
            IncrementScript,
            new RedisKey[] { BuildRedisKey(request, windowStart) },
            new RedisValue[] { (_options.WindowSeconds * 1000).ToString(CultureInfo.InvariantCulture) });

        long count = (long)result;
        int remaining = (int)Math.Max(0, _options.PermitLimit - count);
        TimeSpan retryAfter = resetAtUtc - nowUtc;

        return new GatewayRateLimitDecision(
            count <= _options.PermitLimit,
            _options.PermitLimit,
            remaining,
            resetAtUtc,
            retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.Zero);
    }

    private string BuildRedisKey(GatewayRateLimitRequest request, long windowStart)
    {
        string subject = request.Principal.ApplicationId ?? request.Principal.SubjectId;

        return string.Join(
            ':',
            _options.KeyPrefix,
            request.Principal.TenantId.Value.ToString("N"),
            subject,
            NormalizeRouteKey(request.RouteKey),
            windowStart.ToString(CultureInfo.InvariantCulture));
    }

    private static string NormalizeRouteKey(string routeKey)
    {
        if (string.IsNullOrWhiteSpace(routeKey))
        {
            return "default";
        }

        char[] normalized = routeKey
            .Select(character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-')
            .ToArray();

        string value = new(normalized);
        string trimmed = value.Trim('-');
        return trimmed.Length == 0 ? "default" : trimmed;
    }
}

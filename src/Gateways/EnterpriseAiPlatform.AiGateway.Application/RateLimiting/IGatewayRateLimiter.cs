namespace EnterpriseAiPlatform.AiGateway.Application.RateLimiting;

public interface IGatewayRateLimiter
{
    ValueTask<GatewayRateLimitDecision> CheckAsync(
        GatewayRateLimitRequest request,
        CancellationToken cancellationToken = default);
}

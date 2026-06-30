namespace EnterpriseAiPlatform.AiGateway.Infrastructure.RateLimiting;

public sealed class GatewayRateLimitOptions
{
    public const string SectionName = "AiGateway:RateLimiting";

    public string RedisConnectionString { get; init; } = string.Empty;

    public string KeyPrefix { get; init; } = "eap:ai-gateway:rate-limit";

    public int PermitLimit { get; init; } = 120;

    public int WindowSeconds { get; init; } = 60;
}

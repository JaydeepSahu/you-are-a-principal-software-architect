namespace EnterpriseAiPlatform.AiGateway.Application.RateLimiting;

public sealed record GatewayRateLimitDecision(
    bool IsAllowed,
    int Limit,
    int Remaining,
    DateTimeOffset ResetAtUtc,
    TimeSpan RetryAfter);

namespace EnterpriseAiPlatform.AiGateway.Application.RateLimiting;

public sealed record GatewayRateLimitRequest(
    GatewayPrincipalContext Principal,
    string RouteKey,
    string CorrelationId);

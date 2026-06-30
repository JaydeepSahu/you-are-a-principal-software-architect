namespace EnterpriseAiPlatform.AiGateway.Contracts;

public static class AiGatewayHeaders
{
    public const string CorrelationId = "X-Correlation-ID";
    public const string TenantId = "X-Tenant-ID";
    public const string SubjectId = "X-Subject-ID";
    public const string ApplicationId = "X-Application-ID";
    public const string ForwardedBy = "X-Forwarded-By";
    public const string RateLimitLimit = "X-RateLimit-Limit";
    public const string RateLimitRemaining = "X-RateLimit-Remaining";
    public const string RateLimitReset = "X-RateLimit-Reset";
    public const string RetryAfter = "Retry-After";
}

using System.Globalization;
using EnterpriseAiPlatform.AiGateway.Api;
using EnterpriseAiPlatform.AiGateway.Api.Security;
using EnterpriseAiPlatform.AiGateway.Application;
using EnterpriseAiPlatform.AiGateway.Application.RateLimiting;
using EnterpriseAiPlatform.AiGateway.Application.Telemetry;
using EnterpriseAiPlatform.AiGateway.Contracts;

namespace EnterpriseAiPlatform.AiGateway.Api.Middleware;

public sealed class GatewayRateLimitingMiddleware
{
    private static readonly Action<ILogger<GatewayRateLimitingMiddleware>, Exception?> InvalidPrincipalLog
        = LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(1, "InvalidPrincipal"),
            "AI gateway rejected an authenticated request with an invalid principal.");

    private static readonly Action<ILogger<GatewayRateLimitingMiddleware>, string, string?, string, Exception?> RateLimitRejectedLog
        = LoggerMessage.Define<string, string?, string>(
            LogLevel.Warning,
            new EventId(2, "RateLimitRejected"),
            "AI gateway rate limit rejected request for tenant {TenantId}, application {ApplicationId}, route {RouteKey}.");

    private readonly RequestDelegate _next;

    public GatewayRateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        IGatewayPrincipalContextAccessor principalContextAccessor,
        IGatewayRateLimiter rateLimiter,
        IGatewayMetrics metrics,
        ILogger<GatewayRateLimitingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(principalContextAccessor);
        ArgumentNullException.ThrowIfNull(rateLimiter);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(logger);

        if (!IsGatewayRequest(httpContext) || httpContext.User.Identity?.IsAuthenticated != true)
        {
            await _next(httpContext);
            return;
        }

        GatewayPrincipalContext principal;
        try
        {
            principal = principalContextAccessor.Current;
        }
        catch (InvalidOperationException exception)
        {
            InvalidPrincipalLog(logger, exception);
            await Results.Problem(
                title: "ai_gateway.invalid_principal",
                detail: "The authenticated principal is missing required gateway claims.",
                statusCode: StatusCodes.Status403Forbidden)
                .ExecuteAsync(httpContext);
            return;
        }

        string routeKey = GatewayRouteKeys.FromHttpContext(httpContext);
        metrics.RecordRequestReceived(principal, routeKey);

        GatewayRateLimitDecision decision = await rateLimiter.CheckAsync(
            new GatewayRateLimitRequest(principal, routeKey, httpContext.GetCorrelationId()),
            httpContext.RequestAborted);

        ApplyRateLimitHeaders(httpContext, decision);

        if (decision.IsAllowed)
        {
            await _next(httpContext);
            return;
        }

        metrics.RecordRateLimitRejected(principal, routeKey);
        RateLimitRejectedLog(
            logger,
            principal.TenantId.Value.ToString("D"),
            principal.ApplicationId,
            routeKey,
            null);

        await Results.Problem(
            title: "ai_gateway.rate_limit_exceeded",
            detail: "The AI gateway rate limit for this tenant and principal has been exceeded.",
            statusCode: StatusCodes.Status429TooManyRequests)
            .ExecuteAsync(httpContext);
    }

    private static void ApplyRateLimitHeaders(HttpContext httpContext, GatewayRateLimitDecision decision)
    {
        httpContext.Response.Headers[AiGatewayHeaders.RateLimitLimit] = decision.Limit.ToString(CultureInfo.InvariantCulture);
        httpContext.Response.Headers[AiGatewayHeaders.RateLimitRemaining] = decision.Remaining.ToString(CultureInfo.InvariantCulture);
        httpContext.Response.Headers[AiGatewayHeaders.RateLimitReset] = decision.ResetAtUtc.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        if (!decision.IsAllowed)
        {
            int retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(decision.RetryAfter.TotalSeconds));
            httpContext.Response.Headers[AiGatewayHeaders.RetryAfter] = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
        }
    }

    private static bool IsGatewayRequest(HttpContext httpContext)
    {
        return httpContext.Request.Path.StartsWithSegments("/api/v1/ai", StringComparison.OrdinalIgnoreCase);
    }
}

using System.Diagnostics;
using EnterpriseAiPlatform.AiGateway.Api.Security;
using EnterpriseAiPlatform.Identity.Contracts;

namespace EnterpriseAiPlatform.AiGateway.Api.Middleware;

public sealed class GatewayRequestLoggingMiddleware
{
    private static readonly Action<ILogger<GatewayRequestLoggingMiddleware>, string, string?, int, long, Exception?> RequestCompletedLog
        = LoggerMessage.Define<string, string?, int, long>(
            LogLevel.Information,
            new EventId(1, "AIGatewayRequestCompleted"),
            "AI gateway request completed: {Method} {Path} -> {StatusCode} in {ElapsedMilliseconds} ms.");

    private readonly RequestDelegate _next;
    private readonly ILogger<GatewayRequestLoggingMiddleware> _logger;

    public GatewayRequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<GatewayRequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!IsGatewayRequest(httpContext))
        {
            await _next(httpContext);
            return;
        }

        string correlationId = httpContext.GetCorrelationId();
        using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["TenantId"] = httpContext.User.FindFirst(IdentityClaimTypes.TenantId)?.Value,
            ["ApplicationId"] = httpContext.User.FindFirst(IdentityClaimTypes.ApplicationId)?.Value,
            ["SubjectId"] = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? httpContext.User.FindFirst("sub")?.Value
        });

        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(httpContext);
        }
        finally
        {
            stopwatch.Stop();
            RequestCompletedLog(
                _logger,
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                httpContext.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                null);
        }
    }

    private static bool IsGatewayRequest(HttpContext httpContext)
    {
        return httpContext.Request.Path.StartsWithSegments("/api/v1/ai", StringComparison.OrdinalIgnoreCase);
    }
}

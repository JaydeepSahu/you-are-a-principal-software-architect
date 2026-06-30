namespace EnterpriseAiPlatform.AiGateway.Api.Security;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        IHeaderDictionary headers = httpContext.Response.Headers;
        headers.TryAdd("X-Content-Type-Options", "nosniff");
        headers.TryAdd("X-Frame-Options", "DENY");
        headers.TryAdd("Referrer-Policy", "no-referrer");
        headers.TryAdd("Cache-Control", "no-store");
        headers.TryAdd("Pragma", "no-cache");
        headers.TryAdd("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'; base-uri 'none'");
        headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

        await _next(httpContext);
    }
}

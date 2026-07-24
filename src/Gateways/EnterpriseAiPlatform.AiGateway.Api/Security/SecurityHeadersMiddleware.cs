using EnterpriseAiPlatform.ServiceDefaults;

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
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Cache-Control"] = "no-store";
        headers["Pragma"] = "no-cache";
        // Path-aware CSP: relaxed for /scalar/* and /openapi/*, strict for all API paths
        headers["Content-Security-Policy"] = ServiceDefaultsExtensions.GetContentSecurityPolicy(httpContext.Request.Path);
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        await _next(httpContext);
    }
}

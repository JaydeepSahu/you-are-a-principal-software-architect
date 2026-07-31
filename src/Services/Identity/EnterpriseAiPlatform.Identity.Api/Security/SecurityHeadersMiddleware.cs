using EnterpriseAiPlatform.ServiceDefaults;

namespace EnterpriseAiPlatform.Identity.Api.Security;

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

        var path = httpContext.Request.Path;
        var isDocPath = path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase)
                     || path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase)
                     || path.StartsWithSegments("/_content", StringComparison.OrdinalIgnoreCase);

        IHeaderDictionary headers = httpContext.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Content-Security-Policy"] = ServiceDefaultsExtensions.GetContentSecurityPolicy(path);
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        if (!isDocPath)
        {
            headers["Cache-Control"] = "no-store";
            headers["Pragma"] = "no-cache";
        }

        await _next(httpContext);
    }
}

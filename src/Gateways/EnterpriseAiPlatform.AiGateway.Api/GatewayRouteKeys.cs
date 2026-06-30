namespace EnterpriseAiPlatform.AiGateway.Api;

public static class GatewayRouteKeys
{
    public static string FromHttpContext(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        string path = httpContext.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/api/v1/ai/", StringComparison.OrdinalIgnoreCase))
        {
            return path["/api/v1/ai/".Length..];
        }

        if (string.Equals(path, "/api/v1/ai", StringComparison.OrdinalIgnoreCase))
        {
            return "root";
        }

        return path.Trim('/').Length == 0 ? "root" : path.Trim('/');
    }
}

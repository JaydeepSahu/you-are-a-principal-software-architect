namespace EnterpriseAiPlatform.Identity.Api.Security;

public static class CorrelationIdExtensions
{
    private const string HeaderName = "X-Correlation-ID";

    public static string GetCorrelationId(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext.Request.Headers.TryGetValue(HeaderName, out Microsoft.Extensions.Primitives.StringValues value)
            && !string.IsNullOrWhiteSpace(value.FirstOrDefault()))
        {
            return value.First()!;
        }

        return httpContext.TraceIdentifier;
    }

    public static void ApplyCorrelationId(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        string correlationId = httpContext.GetCorrelationId();
        httpContext.Response.Headers[HeaderName] = correlationId;
    }
}

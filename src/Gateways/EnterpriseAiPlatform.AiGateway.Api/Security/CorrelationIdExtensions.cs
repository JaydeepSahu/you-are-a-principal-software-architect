using EnterpriseAiPlatform.AiGateway.Contracts;

namespace EnterpriseAiPlatform.AiGateway.Api.Security;

public static class CorrelationIdExtensions
{
    public static string GetCorrelationId(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext.Request.Headers.TryGetValue(AiGatewayHeaders.CorrelationId, out Microsoft.Extensions.Primitives.StringValues value)
            && !string.IsNullOrWhiteSpace(value.FirstOrDefault()))
        {
            return value.First()!;
        }

        return httpContext.TraceIdentifier;
    }

    public static void ApplyCorrelationId(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.Headers[AiGatewayHeaders.CorrelationId] = httpContext.GetCorrelationId();
    }
}

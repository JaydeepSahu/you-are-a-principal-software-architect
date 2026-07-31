using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace EnterpriseAiPlatform.PortalBff.Api.Endpoints;

internal static class PortalBackendProxy
{
    public static bool TryGetBackendBaseUri(
        IConfiguration configuration,
        string backendName,
        out Uri baseUri)
    {
        string? configured = configuration[$"PortalBff:Backends:{backendName}"];
        if (Uri.TryCreate(configured, UriKind.Absolute, out var parsed)
            && parsed.Scheme is "http" or "https")
        {
            baseUri = parsed;
            return true;
        }

        baseUri = new Uri("http://127.0.0.1");
        return false;
    }

    public static HttpRequestMessage CreateRequest(
        HttpContext httpContext,
        HttpMethod method,
        Uri baseUri,
        string relativePath)
    {
        var request = new HttpRequestMessage(method, new Uri(baseUri, relativePath.TrimStart('/')));

        if (httpContext.Request.Headers.TryGetValue("Authorization", out var authorization)
            && AuthenticationHeaderValue.TryParse(authorization.FirstOrDefault(), out var authHeader))
        {
            request.Headers.Authorization = authHeader;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId.ToArray());
        }

        return request;
    }

    public static IResult BackendNotConfigured(string backendName)
        => Results.Problem(
            title: "PortalBff.BackendNotConfigured",
            detail: $"Portal backend '{backendName}' is not configured.",
            statusCode: StatusCodes.Status503ServiceUnavailable);

    public static async Task<IResult> ForwardJsonAsync(
        HttpContext httpContext,
        IHttpClientFactory httpClientFactory,
        Uri baseUri,
        string relativePath,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(httpContext, HttpMethod.Get, baseUri, relativePath);
        using var response = await httpClientFactory.CreateClient().SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        string contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        return Results.Content(body, contentType, statusCode: (int)response.StatusCode);
    }
}

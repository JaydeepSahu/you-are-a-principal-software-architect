using System.Diagnostics;
using EnterpriseAiPlatform.AiGateway.Api;
using EnterpriseAiPlatform.AiGateway.Application;
using EnterpriseAiPlatform.AiGateway.Application.Telemetry;
using EnterpriseAiPlatform.AiGateway.Api.Security;
using EnterpriseAiPlatform.AiGateway.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace EnterpriseAiPlatform.AiGateway.Api.Forwarding;

public sealed class InternalRequestForwarder : IInternalRequestForwarder
{
    public const string HttpClientName = "ai-gateway-internal-forwarder";

    private static readonly HashSet<string> SuppressedRequestHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade",
        "Host",
        "Content-Length",
        "Authorization",
        "Cookie",
        "X-API-Key",
        AiGatewayHeaders.TenantId,
        AiGatewayHeaders.SubjectId,
        AiGatewayHeaders.ApplicationId,
        AiGatewayHeaders.ForwardedBy
    };

    private static readonly HashSet<string> SuppressedResponseHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade"
    };

    private static readonly Action<ILogger<InternalRequestForwarder>, string, Exception?> ForwardingFailedLog
        = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, "InternalForwardingFailed"),
            "AI gateway internal forwarding failed for route {RouteKey}.");

    private readonly HttpClient _httpClient;
    private readonly InternalForwardingOptions _options;
    private readonly IGatewayMetrics _metrics;
    private readonly ILogger<InternalRequestForwarder> _logger;

    public InternalRequestForwarder(
        IHttpClientFactory httpClientFactory,
        IOptions<InternalForwardingOptions> options,
        IGatewayMetrics metrics,
        ILogger<InternalRequestForwarder> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _options = options.Value;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task ForwardAsync(
        HttpContext httpContext,
        string path,
        GatewayPrincipalContext principal,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(principal);

        string routeKey = GatewayRouteKeys.FromHttpContext(httpContext);
        Uri targetUri = BuildTargetUri(_options.GetBaseAddress(), path, httpContext.Request.QueryString);
        using HttpRequestMessage requestMessage = CreateForwardRequest(httpContext, targetUri, principal);
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            using HttpResponseMessage responseMessage = await _httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            stopwatch.Stop();
            await WriteForwardResponseAsync(httpContext, responseMessage, cancellationToken);

            GatewayForwardingOutcome outcome = responseMessage.IsSuccessStatusCode || (int)responseMessage.StatusCode < StatusCodes.Status500InternalServerError
                ? GatewayForwardingOutcome.Succeeded
                : GatewayForwardingOutcome.Failed;

            _metrics.RecordForwardingCompleted(principal, routeKey, outcome, stopwatch.Elapsed, (int)responseMessage.StatusCode);
        }
        catch (OperationCanceledException) when (!httpContext.RequestAborted.IsCancellationRequested)
        {
            stopwatch.Stop();
            _metrics.RecordForwardingCompleted(principal, routeKey, GatewayForwardingOutcome.TimedOut, stopwatch.Elapsed, StatusCodes.Status504GatewayTimeout);
            await WriteGatewayProblemAsync(
                httpContext,
                "ai_gateway.forwarding_timeout",
                "The internal AI request processor did not respond before the gateway timeout elapsed.",
                StatusCodes.Status504GatewayTimeout);
        }
        catch (HttpRequestException exception)
        {
            stopwatch.Stop();
            ForwardingFailedLog(_logger, routeKey, exception);
            _metrics.RecordForwardingCompleted(principal, routeKey, GatewayForwardingOutcome.Unavailable, stopwatch.Elapsed, StatusCodes.Status502BadGateway);
            await WriteGatewayProblemAsync(
                httpContext,
                "ai_gateway.forwarding_unavailable",
                "The internal AI request processor is unavailable.",
                StatusCodes.Status502BadGateway);
        }
    }

    private HttpRequestMessage CreateForwardRequest(
        HttpContext httpContext,
        Uri targetUri,
        GatewayPrincipalContext principal)
    {
        HttpRequestMessage requestMessage = new(new HttpMethod(httpContext.Request.Method), targetUri);

        bool hasBody = httpContext.Request.ContentLength.GetValueOrDefault() > 0
            || httpContext.Request.Headers.ContainsKey("Transfer-Encoding");

        if (hasBody)
        {
            requestMessage.Content = new StreamContent(httpContext.Request.Body);
        }

        CopyRequestHeaders(httpContext, requestMessage);
        ApplyGatewayHeaders(httpContext, requestMessage, principal);

        return requestMessage;
    }

    private static void CopyRequestHeaders(HttpContext httpContext, HttpRequestMessage requestMessage)
    {
        foreach (KeyValuePair<string, StringValues> header in httpContext.Request.Headers)
        {
            if (SuppressedRequestHeaders.Contains(header.Key))
            {
                continue;
            }

            string[] values = header.Value.ToArray()!;
            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, values)
                && requestMessage.Content is not null)
            {
                requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, values);
            }
        }
    }

    private void ApplyGatewayHeaders(
        HttpContext httpContext,
        HttpRequestMessage requestMessage,
        GatewayPrincipalContext principal)
    {
        requestMessage.Headers.Remove(AiGatewayHeaders.CorrelationId);
        requestMessage.Headers.TryAddWithoutValidation(AiGatewayHeaders.CorrelationId, httpContext.GetCorrelationId());
        requestMessage.Headers.TryAddWithoutValidation(AiGatewayHeaders.TenantId, principal.TenantId.Value.ToString("D"));
        requestMessage.Headers.TryAddWithoutValidation(AiGatewayHeaders.SubjectId, principal.SubjectId);
        requestMessage.Headers.TryAddWithoutValidation(AiGatewayHeaders.ForwardedBy, _options.ForwardedBy);

        if (!string.IsNullOrWhiteSpace(principal.ApplicationId))
        {
            requestMessage.Headers.TryAddWithoutValidation(AiGatewayHeaders.ApplicationId, principal.ApplicationId);
        }
    }

    private static async Task WriteForwardResponseAsync(
        HttpContext httpContext,
        HttpResponseMessage responseMessage,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = (int)responseMessage.StatusCode;

        foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Headers)
        {
            if (!SuppressedResponseHeaders.Contains(header.Key))
            {
                httpContext.Response.Headers[header.Key] = header.Value.ToArray();
            }
        }

        foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Content.Headers)
        {
            if (!SuppressedResponseHeaders.Contains(header.Key))
            {
                httpContext.Response.Headers[header.Key] = header.Value.ToArray();
            }
        }

        httpContext.Response.Headers.Remove("transfer-encoding");
        await responseMessage.Content.CopyToAsync(httpContext.Response.Body, cancellationToken);
    }

    private static Task WriteGatewayProblemAsync(
        HttpContext httpContext,
        string title,
        string detail,
        int statusCode)
    {
        return Results.Problem(
            title: title,
            detail: detail,
            statusCode: statusCode)
            .ExecuteAsync(httpContext);
    }

    private static Uri BuildTargetUri(Uri baseAddress, string path, QueryString queryString)
    {
        Uri normalizedBaseAddress = baseAddress.AbsoluteUri.EndsWith('/')
            ? baseAddress
            : new Uri($"{baseAddress.AbsoluteUri}/", UriKind.Absolute);

        string normalizedPath = string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.TrimStart('/');

        UriBuilder uriBuilder = new(new Uri(normalizedBaseAddress, normalizedPath));
        if (queryString.HasValue)
        {
            uriBuilder.Query = queryString.Value![1..];
        }

        return uriBuilder.Uri;
    }
}

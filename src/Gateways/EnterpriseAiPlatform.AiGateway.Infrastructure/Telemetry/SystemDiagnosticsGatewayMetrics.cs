using System.Diagnostics.Metrics;
using EnterpriseAiPlatform.AiGateway.Application;
using EnterpriseAiPlatform.AiGateway.Application.Telemetry;

namespace EnterpriseAiPlatform.AiGateway.Infrastructure.Telemetry;

public sealed class SystemDiagnosticsGatewayMetrics : IGatewayMetrics
{
    public const string MeterName = "EnterpriseAiPlatform.AiGateway";

    private static readonly Meter GatewayMeter = new(MeterName, "1.0.0");

    private static readonly Counter<long> RequestsReceived = GatewayMeter.CreateCounter<long>(
        "ai_gateway_requests_received_total",
        description: "Total AI gateway requests received.");

    private static readonly Counter<long> RateLimitRejections = GatewayMeter.CreateCounter<long>(
        "ai_gateway_rate_limit_rejections_total",
        description: "Total AI gateway requests rejected by rate limiting.");

    private static readonly Counter<long> ForwardingCompleted = GatewayMeter.CreateCounter<long>(
        "ai_gateway_forwarding_completed_total",
        description: "Total AI gateway forwarding attempts completed.");

    private static readonly Histogram<double> ForwardingDuration = GatewayMeter.CreateHistogram<double>(
        "ai_gateway_forwarding_duration_ms",
        unit: "ms",
        description: "AI gateway internal forwarding duration in milliseconds.");

    public void RecordRequestReceived(GatewayPrincipalContext principal, string routeKey)
    {
        RequestsReceived.Add(1, CreateTags(principal, routeKey));
    }

    public void RecordRateLimitRejected(GatewayPrincipalContext principal, string routeKey)
    {
        RateLimitRejections.Add(1, CreateTags(principal, routeKey));
    }

    public void RecordForwardingCompleted(
        GatewayPrincipalContext principal,
        string routeKey,
        GatewayForwardingOutcome outcome,
        TimeSpan duration,
        int statusCode)
    {
        KeyValuePair<string, object?>[] tags =
        [
            .. CreateTags(principal, routeKey),
            new("outcome", outcome.ToString()),
            new("status_code", statusCode)
        ];

        ForwardingCompleted.Add(1, tags);
        ForwardingDuration.Record(duration.TotalMilliseconds, tags);
    }

    private static KeyValuePair<string, object?>[] CreateTags(
        GatewayPrincipalContext principal,
        string routeKey)
    {
        return
        [
            new("tenant_id", principal.TenantId.Value.ToString("D")),
            new("application_id", principal.ApplicationId ?? string.Empty),
            new("route_key", routeKey)
        ];
    }
}

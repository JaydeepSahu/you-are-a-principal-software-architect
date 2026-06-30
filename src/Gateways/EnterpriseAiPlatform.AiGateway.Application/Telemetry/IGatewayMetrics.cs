namespace EnterpriseAiPlatform.AiGateway.Application.Telemetry;

public interface IGatewayMetrics
{
    void RecordRequestReceived(GatewayPrincipalContext principal, string routeKey);

    void RecordRateLimitRejected(GatewayPrincipalContext principal, string routeKey);

    void RecordForwardingCompleted(
        GatewayPrincipalContext principal,
        string routeKey,
        GatewayForwardingOutcome outcome,
        TimeSpan duration,
        int statusCode);
}

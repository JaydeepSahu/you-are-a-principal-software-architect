using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EnterpriseAiPlatform.ServiceDefaults;

public static class TelemetryConstants
{
    public const string ServiceName = "EnterpriseAiPlatform";
    public const string ActivitySourceName = "EnterpriseAiPlatform.Core";
    public const string MeterName = "EnterpriseAiPlatform.Metrics";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, "1.0.0");
    public static readonly Meter Meter = new(MeterName, "1.0.0");

    // Custom Meters
    public static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>(
        "enterprise_ai_requests_total",
        description: "Total number of AI control plane API requests.");

    public static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>(
        "enterprise_ai_request_duration_seconds",
        unit: "s",
        description: "Latency duration of AI control plane requests in seconds.");

    public static readonly Counter<long> TokenCounter = Meter.CreateCounter<long>(
        "enterprise_ai_token_usage_total",
        description: "Total tokens consumed across AI providers.");

    public static readonly Counter<long> PolicyViolationCounter = Meter.CreateCounter<long>(
        "enterprise_ai_policy_violations_total",
        description: "Total policy violations intercepted.");

    // Standard Attributes / Tag Keys
    public const string TagTenantId = "tenant.id";
    public const string TagCorrelationId = "correlation.id";
    public const string TagProviderName = "provider.name";
    public const string TagModelId = "model.id";
    public const string TagPromptTokens = "prompt.tokens";
    public const string TagCompletionTokens = "completion.tokens";
    public const string TagStatusCode = "http.status_code";
}

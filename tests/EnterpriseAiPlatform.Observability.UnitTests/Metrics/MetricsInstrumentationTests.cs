using EnterpriseAiPlatform.ServiceDefaults;
using Xunit;

namespace EnterpriseAiPlatform.Observability.UnitTests.Metrics;

public class MetricsInstrumentationTests
{
    [Fact]
    public void MetricCounters_RecordRequestAndTokenUsageWithoutExceptions()
    {
        // Act & Assert
        TelemetryConstants.RequestCounter.Add(1, KeyValuePair.Create<string, object?>(TelemetryConstants.TagTenantId, "tenant-456"));
        TelemetryConstants.TokenCounter.Add(150, KeyValuePair.Create<string, object?>(TelemetryConstants.TagProviderName, "Anthropic"));
        TelemetryConstants.RequestDuration.Record(0.125, KeyValuePair.Create<string, object?>(TelemetryConstants.TagModelId, "claude-3-5-sonnet"));

        Assert.NotNull(TelemetryConstants.Meter);
    }
}

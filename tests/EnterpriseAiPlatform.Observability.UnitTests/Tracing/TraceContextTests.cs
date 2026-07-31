using System.Diagnostics;
using EnterpriseAiPlatform.ServiceDefaults;
using Xunit;

namespace EnterpriseAiPlatform.Observability.UnitTests.Tracing;

public class TraceContextTests
{
    [Fact]
    public void ActivitySource_CreatesSpanWithTenantAndProviderTags()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == TelemetryConstants.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        // Act
        using var activity = TelemetryConstants.ActivitySource.StartActivity("AiGateway.RouteRequest");
        activity?.SetTag(TelemetryConstants.TagTenantId, "tenant-test");
        activity?.SetTag(TelemetryConstants.TagProviderName, "AzureOpenAi");

        // Assert
        Assert.NotNull(activity);
        Assert.Equal("AiGateway.RouteRequest", activity.OperationName);
        Assert.Equal("tenant-test", activity.GetTagItem(TelemetryConstants.TagTenantId));
        Assert.Equal("AzureOpenAi", activity.GetTagItem(TelemetryConstants.TagProviderName));
    }
}

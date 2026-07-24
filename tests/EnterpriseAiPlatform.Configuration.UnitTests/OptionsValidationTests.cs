using EnterpriseAiPlatform.AiGateway.Infrastructure.Resilience;
using EnterpriseAiPlatform.Application.Abstractions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace EnterpriseAiPlatform.Configuration.UnitTests;

public class OptionsValidationTests
{
    [Fact]
    public void ResilienceOptions_BindsConfiguredValuesCorrectly()
    {
        // Arrange
        var customOptions = new ResilienceOptions
        {
            OpenRecoveryTimeoutSeconds = 30,
            MinimumThresholdCount = 5,
            FailureRateTripThresholdPercentage = 60.0
        };

        var optionsWrapper = Options.Create(customOptions);

        // Act
        var manager = new ProviderCircuitBreakerManager(optionsWrapper);
        var status = manager.GetProviderStatus("AzureOpenAi");

        // Assert
        Assert.NotNull(status);
        Assert.Equal("Anthropic", status.PrimaryFallbackProvider);
    }
}

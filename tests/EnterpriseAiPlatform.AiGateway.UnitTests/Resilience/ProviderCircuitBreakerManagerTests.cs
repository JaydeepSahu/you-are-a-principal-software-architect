using EnterpriseAiPlatform.AiGateway.Application.Abstractions;
using EnterpriseAiPlatform.AiGateway.Infrastructure.Resilience;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.AiGateway.UnitTests.Resilience;

public class ProviderCircuitBreakerManagerTests
{
    [Fact]
    public async Task RouteWithResilienceAsync_WhenPrimaryOpen_ReroutesToFallbackProvider()
    {
        // Arrange
        var manager = new ProviderCircuitBreakerManager();
        var tenantId = TenantId.From(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        // Trip AzureOpenAi circuit breaker to Open state
        for (int i = 0; i < 5; i++)
        {
            manager.RecordOutcome("AzureOpenAi", isSuccess: false);
        }

        var status = manager.GetProviderStatus("AzureOpenAi");
        Assert.Equal(CircuitState.Open, status.State);

        var request = new ResilientRoutingRequest(
            PreferredProvider: "AzureOpenAi",
            ModelId: "gpt-4o",
            PromptPayload: "Generate code refactoring plan"
        );

        // Act
        var result = await manager.RouteWithResilienceAsync(
            tenantId,
            request,
            (targetProvider, ct) => Task.FromResult(Result<string>.Success($"Response from {targetProvider}"))
        );

        // Assert
        Assert.True(result.IsSuccess);
        var routingResult = result.Value;
        Assert.True(routingResult.WasFallbackUsed);
        Assert.Equal("Anthropic", routingResult.ExecutedProvider);
    }
}

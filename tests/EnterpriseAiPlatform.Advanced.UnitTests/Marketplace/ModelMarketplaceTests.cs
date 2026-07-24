using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Infrastructure.Marketplace;
using Xunit;

namespace EnterpriseAiPlatform.Advanced.UnitTests.Marketplace;

public class ModelMarketplaceTests
{
    [Fact]
    public async Task PublishAndSearchMarketplaceAsync_FiltersAndSortsByBenchmarkScore()
    {
        // Arrange
        var marketplace = new ModelMarketplace();

        var modelEntry = new MarketplaceEntry(
            Guid.NewGuid(), "DeepSeek-Coder-V2", "1.0.0", "Model",
            "State of the art open code model", 0.92, 120.0, 0.002m, "tenant-1", true
        );
        var agentEntry = new MarketplaceEntry(
            Guid.NewGuid(), "SecurityAuditAgent", "2.1.0", "Agent",
            "Automated vulnerability scanner agent", 0.96, 350.0, 0.005m, "tenant-1", true
        );

        // Act
        await marketplace.PublishEntryAsync(modelEntry);
        await marketplace.PublishEntryAsync(agentEntry);

        var searchResult = await marketplace.SearchMarketplaceAsync(category: "Agent");

        // Assert
        Assert.True(searchResult.IsSuccess);
        Assert.Single(searchResult.Value);
        Assert.Equal("SecurityAuditAgent", searchResult.Value[0].Name);
    }
}

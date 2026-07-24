using EnterpriseAiPlatform.Knowledge.Infrastructure.Rag;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Advanced.UnitTests.Rag;

public class HybridRagEngineTests
{
    [Fact]
    public async Task IngestAndHybridSearchAsync_ReturnsRrfRankedResults()
    {
        // Arrange
        var engine = new HybridRagEngine();
        var tenantId = TenantId.From("tenant-rag");

        // Act
        await engine.IngestDocumentAsync(
            tenantId,
            "Clean Architecture Guidelines",
            "docs/clean-arch.md",
            "Clean architecture enforces inner domain boundaries with zero external infrastructure dependencies."
        );

        var searchResult = await engine.HybridSearchAsync(tenantId, "Clean architecture domain");

        // Assert
        Assert.True(searchResult.IsSuccess);
        Assert.NotEmpty(searchResult.Value);
        var top = searchResult.Value[0];
        Assert.True(top.CombinedScore > 0);
        Assert.Contains("Clean architecture", top.TextContent);
    }
}

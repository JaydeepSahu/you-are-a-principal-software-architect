using EnterpriseAiPlatform.Caching.Engine;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Caching.UnitTests;

public class SemanticCacheEngineTests
{
    [Fact]
    public async Task GetCachedResponseAsync_WhenSemanticallyEquivalentPromptCached_ReturnsCacheHit()
    {
        // Arrange
        var cache = new SemanticCacheEngine();
        var tenantId = TenantId.From("tenant-cache-test");
        string originalPrompt = "Explain Clean Architecture invariants in detail";
        string cachedResponse = "Clean Architecture maintains inner domain boundary isolation.";

        await cache.CacheResponseAsync(tenantId, originalPrompt, cachedResponse, "azure-gpt-4o");

        string semanticallyEquivalentPrompt = "Explain Clean Architecture invariants in detail";

        // Act
        var result = await cache.GetCachedResponseAsync(tenantId, semanticallyEquivalentPrompt, minSimilarityThreshold: 0.95);

        // Assert
        Assert.True(result.IsSuccess);
        var hit = result.Value;
        Assert.True(hit.IsHit);
        Assert.NotNull(hit.Entry);
        Assert.Equal(cachedResponse, hit.Entry.ResponsePayload);
        Assert.True(hit.CosineSimilarity >= 0.95);
    }

    [Fact]
    public async Task GetCachedResponseAsync_WhenCompletelyDifferentPrompt_ReturnsCacheMiss()
    {
        // Arrange
        var cache = new SemanticCacheEngine();
        var tenantId = TenantId.From("tenant-cache-test");
        await cache.CacheResponseAsync(tenantId, "Explain Clean Architecture", "Response A", "azure-gpt-4o");

        // Act
        var result = await cache.GetCachedResponseAsync(tenantId, "What is the weather in Tokyo today?", minSimilarityThreshold: 0.95);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsHit);
    }
}

using EnterpriseAiPlatform.LocalModel.Infrastructure.Cluster;
using Xunit;

namespace EnterpriseAiPlatform.Advanced.UnitTests.Cluster;

public class GpuClusterManagerTests
{
    [Fact]
    public async Task GetClusterStatusAsync_ReturnsHealthyGpuNodes()
    {
        // Arrange
        var manager = new GpuClusterManager();

        // Act
        var result = await manager.GetClusterStatusAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Count >= 2);
        Assert.All(result.Value, node => Assert.True(node.IsHealthy));
    }
}

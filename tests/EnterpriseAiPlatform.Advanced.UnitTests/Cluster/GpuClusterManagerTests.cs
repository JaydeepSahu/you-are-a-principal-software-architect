using EnterpriseAiPlatform.LocalModel.Infrastructure.Cluster;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EnterpriseAiPlatform.Advanced.UnitTests.Cluster;

public class GpuClusterManagerTests
{
    [Fact]
    public async Task GetClusterStatusAsync_ReturnsHealthyGpuNodes()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LocalModel:GpuNodes:0:NodeId"] = "gpu-node-test-01",
                ["LocalModel:GpuNodes:0:GpuModel"] = "NVIDIA L40S",
                ["LocalModel:GpuNodes:0:TotalVramMb"] = "49152",
                ["LocalModel:GpuNodes:0:UsedVramMb"] = "8192",
                ["LocalModel:GpuNodes:0:GpuUtilizationPercentage"] = "17.5",
                ["LocalModel:GpuNodes:0:ActiveInferenceSlots"] = "1",
                ["LocalModel:GpuNodes:0:LoadedModels:0"] = "local-test-model",
                ["LocalModel:GpuNodes:0:IsHealthy"] = "true"
            })
            .Build();
        var manager = new GpuClusterManager(configuration);

        // Act
        var result = await manager.GetClusterStatusAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.All(result.Value, node => Assert.True(node.IsHealthy));
    }
}

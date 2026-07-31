using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.Extensions.Configuration;

namespace EnterpriseAiPlatform.LocalModel.Infrastructure.Cluster;

public sealed class GpuClusterManager(IConfiguration configuration) : IGpuClusterManager
{
    private readonly IReadOnlyList<GpuNodeStatus> _nodes =
        configuration.GetSection("LocalModel:GpuNodes").Get<List<GpuNodeStatus>>() ?? [];

    public Task<Result<IReadOnlyList<GpuNodeStatus>>> GetClusterStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<IReadOnlyList<GpuNodeStatus>>.Success(_nodes));
    }

    public Task<Result<GpuNodeStatus>> SelectOptimalNodeForModelAsync(string modelId, CancellationToken cancellationToken = default)
    {
        var healthyNodes = _nodes.Where(n => n.IsHealthy).OrderBy(n => n.GpuUtilizationPercentage).FirstOrDefault();
        if (healthyNodes == null)
        {
            return Task.FromResult(Result<GpuNodeStatus>.Failure<GpuNodeStatus>(new Error("GPU.NoCapacity", "No healthy GPU nodes available.")));
        }

        return Task.FromResult(Result<GpuNodeStatus>.Success(healthyNodes));
    }
}

using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.LocalModel.Infrastructure.Cluster;

public sealed class GpuClusterManager : IGpuClusterManager
{
    private readonly List<GpuNodeStatus> _nodes = new()
    {
        new GpuNodeStatus("gpu-node-01", "NVIDIA A100-80GB", 81920, 24576, 32.5, 4, new() { "vllm-deepseek-coder", "vllm-llama-3" }, true),
        new GpuNodeStatus("gpu-node-02", "NVIDIA H100-80GB", 81920, 12288, 15.0, 2, new() { "vllm-qwen-2.5-coder" }, true),
        new GpuNodeStatus("gpu-node-03", "NVIDIA L40S-48GB", 49152, 36864, 75.0, 8, new() { "ollama-codegemma" }, true)
    };

    public Task<Result<IReadOnlyList<GpuNodeStatus>>> GetClusterStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<IReadOnlyList<GpuNodeStatus>>.Success(_nodes.AsReadOnly()));
    }

    public Task<Result<GpuNodeStatus>> SelectOptimalNodeForModelAsync(string modelId, CancellationToken cancellationToken = default)
    {
        var healthyNodes = _nodes.Where(n => n.IsHealthy).OrderBy(n => n.GpuUtilizationPercentage).FirstOrDefault();
        if (healthyNodes == null)
        {
            return Task.FromResult(Result<GpuNodeStatus>.Failure(new Error("GPU.NoCapacity", "No healthy GPU nodes available.")));
        }

        return Task.FromResult(Result<GpuNodeStatus>.Success(healthyNodes));
    }
}

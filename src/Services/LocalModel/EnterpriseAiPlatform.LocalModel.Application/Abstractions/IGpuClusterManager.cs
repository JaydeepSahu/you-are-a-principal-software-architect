using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.LocalModel.Application.Abstractions;

public sealed record GpuNodeStatus(
    string NodeId,
    string GpuModel,
    int TotalVramMb,
    int UsedVramMb,
    double GpuUtilizationPercentage,
    int ActiveInferenceSlots,
    List<string> LoadedModels,
    bool IsHealthy);

public interface IGpuClusterManager
{
    Task<Result<IReadOnlyList<GpuNodeStatus>>> GetClusterStatusAsync(CancellationToken cancellationToken = default);
    Task<Result<GpuNodeStatus>> SelectOptimalNodeForModelAsync(string modelId, CancellationToken cancellationToken = default);
}

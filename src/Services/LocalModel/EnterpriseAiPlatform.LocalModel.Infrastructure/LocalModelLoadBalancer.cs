using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Domain;

namespace EnterpriseAiPlatform.LocalModel.Infrastructure;

public sealed class LocalModelLoadBalancer : ILocalModelLoadBalancer
{
    private int _roundRobinCursor;

    public string? SelectProvider(
        IReadOnlyList<ILocalModelPlugin> candidates,
        LocalModelChatRequest request,
        ILocalModelHealthStore healthStore)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var strategy = candidates[0].Descriptor.DefaultStrategy;
        if (request.PreferredGpuIds is { Count: > 0 })
        {
            var gpuAware = candidates
                .Where(candidate => candidate.Descriptor.Gpus.Count == 0 || candidate.Descriptor.Gpus.Any(gpu => request.PreferredGpuIds.Contains(gpu.GpuId, StringComparer.OrdinalIgnoreCase)))
                .OrderByDescending(candidate => candidate.Descriptor.Gpus.Count)
                .ThenBy(candidate => healthStore.GetSnapshot(candidate.Descriptor.ProviderKey).ActiveRequests)
                .ToArray();

            if (gpuAware.Length > 0)
            {
                return gpuAware[0].Descriptor.ProviderKey;
            }
        }

        return strategy switch
        {
            LocalModelLoadBalancingStrategy.LeastLoaded => candidates
                .OrderBy(candidate => healthStore.GetSnapshot(candidate.Descriptor.ProviderKey).ActiveRequests)
                .ThenBy(candidate => candidate.Descriptor.ProviderKey, StringComparer.OrdinalIgnoreCase)
                .First().Descriptor.ProviderKey,
            LocalModelLoadBalancingStrategy.GpuAware => candidates
                .OrderByDescending(candidate => candidate.Descriptor.Gpus.Count)
                .ThenBy(candidate => healthStore.GetSnapshot(candidate.Descriptor.ProviderKey).ActiveRequests)
                .First().Descriptor.ProviderKey,
            _ => candidates[Interlocked.Increment(ref _roundRobinCursor) % candidates.Count].Descriptor.ProviderKey,
        };
    }
}

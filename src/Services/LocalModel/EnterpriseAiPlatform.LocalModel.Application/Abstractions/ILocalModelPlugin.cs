using EnterpriseAiPlatform.LocalModel.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.LocalModel.Application.Abstractions;

public interface ILocalModelProvider
{
    LocalModelDescriptor Descriptor { get; }

    Task<Result<LocalModelChatResponse>> ChatAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default);

    Task<Result<LocalModelStreamingSession>> StreamAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default);

    Task<Result<LocalModelHealthSnapshot>> ProbeHealthAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<string>>> DiscoverModelsAsync(CancellationToken cancellationToken = default);
}

public interface ILocalModelPlugin
{
    LocalModelDescriptor Descriptor { get; }

    ILocalModelProvider CreateProvider(IServiceProvider serviceProvider);
}

public interface ILocalModelPluginRegistry
{
    void Upsert(ILocalModelPlugin plugin);

    bool TryGet(string providerKey, out ILocalModelPlugin plugin);

    bool Remove(string providerKey);

    IReadOnlyCollection<ILocalModelPlugin> GetAll();
}

public interface ILocalModelHealthStore
{
    bool TryAcquire(string providerKey, int capacity, out int activeRequests);

    void Release(string providerKey);

    void RecordHealth(LocalModelHealthSnapshot snapshot);

    LocalModelHealthSnapshot GetSnapshot(string providerKey);

    IReadOnlyCollection<LocalModelHealthSnapshot> GetAllSnapshots();
}

public interface ILocalModelLoadBalancer
{
    string? SelectProvider(
        IReadOnlyList<ILocalModelPlugin> candidates,
        LocalModelChatRequest request,
        ILocalModelHealthStore healthStore);
}

public interface ILocalModelOrchestrator
{
    Task<Result<LocalModelChatResponse>> ChatAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default);

    Task<Result<LocalModelStreamingSession>> StreamAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<string>>> DiscoverModelsAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LocalModelHealthSnapshot>>> GetHealthAsync(CancellationToken cancellationToken = default);
}

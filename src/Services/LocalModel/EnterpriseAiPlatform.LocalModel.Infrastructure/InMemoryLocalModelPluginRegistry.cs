using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Contracts;

namespace EnterpriseAiPlatform.LocalModel.Infrastructure;

public sealed class InMemoryLocalModelPluginRegistry : ILocalModelPluginRegistry
{
    private readonly object _gate = new();
    private readonly Dictionary<string, ILocalModelPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryLocalModelPluginRegistry(IReadOnlyCollection<LocalModelProviderResponse> providers)
    {
        foreach (var provider in providers)
        {
            Upsert(new LocalModelProviderFactory(provider));
        }
    }

    public void Upsert(ILocalModelPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        lock (_gate)
        {
            _plugins[plugin.Descriptor.ProviderKey] = plugin;
        }
    }

    public bool TryGet(string providerKey, out ILocalModelPlugin plugin)
    {
        lock (_gate)
        {
            return _plugins.TryGetValue(providerKey, out plugin!);
        }
    }

    public bool Remove(string providerKey)
    {
        lock (_gate)
        {
            return _plugins.Remove(providerKey);
        }
    }

    public IReadOnlyCollection<ILocalModelPlugin> GetAll()
    {
        lock (_gate)
        {
            return _plugins.Values.ToArray();
        }
    }
}

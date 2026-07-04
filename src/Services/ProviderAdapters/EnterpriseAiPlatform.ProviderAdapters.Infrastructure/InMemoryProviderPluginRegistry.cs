using EnterpriseAiPlatform.ProviderAdapters.Application.Abstractions;

namespace EnterpriseAiPlatform.ProviderAdapters.Infrastructure;

public sealed class InMemoryProviderPluginRegistry : IProviderPluginRegistry
{
    private readonly Dictionary<string, IProviderPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IProviderPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        _plugins[plugin.Descriptor.ProviderKey] = plugin;
    }

    public bool TryGet(string providerKey, out IProviderPlugin plugin)
        => _plugins.TryGetValue(providerKey, out plugin!);

    public IReadOnlyCollection<IProviderPlugin> GetAll()
        => _plugins.Values.ToArray();
}

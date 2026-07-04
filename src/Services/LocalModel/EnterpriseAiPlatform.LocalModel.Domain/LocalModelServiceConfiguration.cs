using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed class LocalModelServiceConfiguration : AggregateRoot<LocalModelServiceConfigurationId>
{
    private readonly Dictionary<string, LocalModelDescriptor> _providers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);

    public LocalModelServiceConfiguration(
        LocalModelServiceConfigurationId id,
        TenantId tenantId,
        string name,
        bool enabled,
        LocalModelLoadBalancingStrategy defaultStrategy,
        IReadOnlyDictionary<string, LocalModelDescriptor>? providers,
        IReadOnlyDictionary<string, string>? metadata,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Name = Normalize(name);
        Enabled = enabled;
        DefaultStrategy = defaultStrategy;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        SetProviders(providers);
        SetMetadata(metadata);
    }

    public TenantId TenantId { get; }

    public string Name { get; private set; }

    public bool Enabled { get; private set; }

    public LocalModelLoadBalancingStrategy DefaultStrategy { get; private set; }

    public IReadOnlyDictionary<string, LocalModelDescriptor> Providers => _providers;

    public IReadOnlyDictionary<string, string> Metadata => _metadata;

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void UpsertProvider(LocalModelDescriptor descriptor, DateTimeOffset updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _providers[descriptor.ProviderKey] = descriptor;
        UpdatedAtUtc = updatedAtUtc;
    }

    public bool RemoveProvider(string providerKey, DateTimeOffset updatedAtUtc)
    {
        var removed = _providers.Remove(providerKey);
        if (removed)
        {
            UpdatedAtUtc = updatedAtUtc;
        }

        return removed;
    }

    private void SetProviders(IReadOnlyDictionary<string, LocalModelDescriptor>? providers)
    {
        _providers.Clear();
        if (providers is null)
        {
            return;
        }

        foreach (var (key, value) in providers)
        {
            if (!string.IsNullOrWhiteSpace(key) && value is not null)
            {
                _providers[key.Trim()] = value;
            }
        }
    }

    private void SetMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        _metadata.Clear();
        if (metadata is null)
        {
            return;
        }

        foreach (var (key, value) in metadata)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _metadata[key.Trim()] = value?.Trim() ?? string.Empty;
            }
        }
    }

    private static string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }
}

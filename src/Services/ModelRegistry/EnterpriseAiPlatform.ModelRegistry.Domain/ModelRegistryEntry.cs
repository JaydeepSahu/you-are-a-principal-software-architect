using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ModelRegistry.Domain;

public sealed class ModelRegistryEntry : AggregateRoot<ModelRegistryEntryId>
{
    private readonly List<ModelCapability> _capabilities = [];
    private readonly Dictionary<string, string> _configuration = new(StringComparer.OrdinalIgnoreCase);

    public ModelRegistryEntry(
        ModelRegistryEntryId id,
        TenantId tenantId,
        ModelProvider provider,
        string providerModelName,
        string displayName,
        string? description,
        IReadOnlyList<ModelCapability> capabilities,
        ModelPricing pricing,
        ModelLatencyProfile latency,
        int contextSize,
        ModelAvailabilityProfile availability,
        ModelHealthProfile health,
        IReadOnlyDictionary<string, string>? configuration,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(contextSize);

        TenantId = tenantId;
        Provider = provider;
        ProviderModelName = Normalize(providerModelName, nameof(providerModelName));
        DisplayName = Normalize(displayName, nameof(displayName));
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Pricing = pricing;
        Latency = latency;
        ContextSize = contextSize;
        Availability = availability;
        Health = health;
        Version = 1;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        SetCapabilities(capabilities);
        SetConfiguration(configuration);
    }

    public TenantId TenantId { get; }

    public ModelProvider Provider { get; private set; }

    public string ProviderModelName { get; private set; }

    public string DisplayName { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyList<ModelCapability> Capabilities => _capabilities;

    public ModelPricing Pricing { get; private set; }

    public ModelLatencyProfile Latency { get; private set; }

    public int ContextSize { get; private set; }

    public ModelAvailabilityProfile Availability { get; private set; }

    public ModelHealthProfile Health { get; private set; }

    public IReadOnlyDictionary<string, string> Configuration => _configuration;

    public int Version { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void Update(
        ModelProvider provider,
        string providerModelName,
        string displayName,
        string? description,
        IReadOnlyList<ModelCapability> capabilities,
        ModelPricing pricing,
        ModelLatencyProfile latency,
        int contextSize,
        ModelAvailabilityProfile availability,
        ModelHealthProfile health,
        IReadOnlyDictionary<string, string>? configuration,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(contextSize);

        Provider = provider;
        ProviderModelName = Normalize(providerModelName, nameof(providerModelName));
        DisplayName = Normalize(displayName, nameof(displayName));
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Pricing = pricing;
        Latency = latency;
        ContextSize = contextSize;
        Availability = availability;
        Health = health;
        UpdatedAtUtc = updatedAtUtc;
        Version++;
        SetCapabilities(capabilities);
        SetConfiguration(configuration);
    }

    private void SetCapabilities(IReadOnlyList<ModelCapability> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        _capabilities.Clear();

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var capability in capabilities)
        {
            ArgumentNullException.ThrowIfNull(capability);
            if (names.Add(capability.Name))
            {
                _capabilities.Add(capability);
            }
        }
    }

    private void SetConfiguration(IReadOnlyDictionary<string, string>? configuration)
    {
        _configuration.Clear();
        if (configuration is null)
        {
            return;
        }

        foreach (var (key, value) in configuration)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            _configuration[key.Trim()] = value?.Trim() ?? string.Empty;
        }
    }

    private static string Normalize(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}

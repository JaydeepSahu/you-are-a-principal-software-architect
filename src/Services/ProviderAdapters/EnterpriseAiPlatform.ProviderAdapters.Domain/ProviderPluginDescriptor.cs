namespace EnterpriseAiPlatform.ProviderAdapters.Domain;

public sealed record ProviderPluginDescriptor
{
    public ProviderPluginDescriptor(
        string providerKey,
        ProviderKind providerKind,
        string displayName,
        string defaultModel,
        ProviderCapability capabilities,
        Uri? endpoint = null,
        ProviderHealthState health = ProviderHealthState.Unknown,
        ProviderResiliencePolicy? resiliencePolicy = null,
        IReadOnlyList<string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultModel);

        ProviderKey = providerKey.Trim();
        ProviderKind = providerKind;
        DisplayName = displayName.Trim();
        DefaultModel = defaultModel.Trim();
        Capabilities = capabilities;
        Endpoint = endpoint;
        Health = health;
        ResiliencePolicy = resiliencePolicy ?? ProviderResiliencePolicy.Default;
        Tags = tags is null
            ? []
            : tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public string ProviderKey { get; }

    public ProviderKind ProviderKind { get; }

    public string DisplayName { get; }

    public string DefaultModel { get; }

    public ProviderCapability Capabilities { get; }

    public Uri? Endpoint { get; }

    public ProviderHealthState Health { get; }

    public ProviderResiliencePolicy ResiliencePolicy { get; }

    public IReadOnlyList<string> Tags { get; }
}

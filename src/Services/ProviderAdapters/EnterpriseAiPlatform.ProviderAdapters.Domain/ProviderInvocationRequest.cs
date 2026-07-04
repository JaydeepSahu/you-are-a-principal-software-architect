namespace EnterpriseAiPlatform.ProviderAdapters.Domain;

public sealed record ProviderInvocationRequest
{
    public ProviderInvocationRequest(
        IReadOnlyList<ProviderMessage> messages,
        string? model = null,
        bool stream = false,
        int? maxOutputTokens = null,
        double? temperature = null,
        double? topP = null,
        string? preferredProviderKey = null,
        IReadOnlyList<string>? fallbackProviderKeys = null,
        TimeSpan? timeout = null,
        int? maxRetries = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(messages);

        Messages = messages.ToArray();
        Model = string.IsNullOrWhiteSpace(model) ? null : model.Trim();
        Stream = stream;
        MaxOutputTokens = maxOutputTokens;
        Temperature = temperature;
        TopP = topP;
        PreferredProviderKey = string.IsNullOrWhiteSpace(preferredProviderKey) ? null : preferredProviderKey.Trim();
        FallbackProviderKeys = fallbackProviderKeys is null
            ? []
            : fallbackProviderKeys.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        Timeout = timeout;
        MaxRetries = maxRetries;
        Metadata = metadata is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ProviderMessage> Messages { get; }

    public string? Model { get; }

    public bool Stream { get; }

    public int? MaxOutputTokens { get; }

    public double? Temperature { get; }

    public double? TopP { get; }

    public string? PreferredProviderKey { get; }

    public IReadOnlyList<string> FallbackProviderKeys { get; }

    public TimeSpan? Timeout { get; }

    public int? MaxRetries { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}

namespace EnterpriseAiPlatform.ProviderAdapters.Domain;

public sealed record ProviderMessage
{
    public ProviderMessage(
        ProviderMessageRole role,
        string content,
        string? name = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Role = role;
        Content = content.Trim();
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        Metadata = metadata is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
    }

    public ProviderMessageRole Role { get; }

    public string Content { get; }

    public string? Name { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}

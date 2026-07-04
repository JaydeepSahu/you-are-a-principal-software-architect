namespace EnterpriseAiPlatform.Knowledge.Domain;

public sealed class KnowledgeDocumentMetadata : SharedKernel.ValueObject
{
    public KnowledgeDocumentMetadata(
        string title,
        string? contentType,
        string? language,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title.Trim();
        ContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim();
        Language = string.IsNullOrWhiteSpace(language) ? "unknown" : language.Trim();
        Attributes = attributes is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(attributes, StringComparer.OrdinalIgnoreCase);
    }

    public string Title { get; }

    public string? ContentType { get; }

    public string Language { get; }

    public IReadOnlyDictionary<string, string> Attributes { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Title;
        yield return ContentType;
        yield return Language;
        foreach (var attribute in Attributes.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            yield return attribute.Key;
            yield return attribute.Value;
        }
    }
}

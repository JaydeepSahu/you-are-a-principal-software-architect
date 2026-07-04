namespace EnterpriseAiPlatform.Knowledge.Domain;

public sealed class KnowledgeSource : SharedKernel.ValueObject
{
    public KnowledgeSource(KnowledgeSourceType type, string externalId, Uri? uri = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);

        Type = type;
        ExternalId = externalId.Trim();
        Uri = uri;
    }

    public KnowledgeSourceType Type { get; }

    public string ExternalId { get; }

    public Uri? Uri { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return ExternalId;
        yield return Uri;
    }
}

namespace EnterpriseAiPlatform.Knowledge.Domain;

public sealed class KnowledgeChunk : SharedKernel.Entity<KnowledgeChunkId>
{
    public KnowledgeChunk(
        KnowledgeChunkId id,
        KnowledgeDocumentId documentId,
        int ordinal,
        string text,
        int tokenCount,
        IReadOnlyDictionary<string, string>? metadata = null)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentOutOfRangeException.ThrowIfNegative(ordinal);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokenCount);

        DocumentId = documentId;
        Ordinal = ordinal;
        Text = text.Trim();
        TokenCount = tokenCount;
        Metadata = metadata is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
    }

    public KnowledgeDocumentId DocumentId { get; }

    public int Ordinal { get; }

    public string Text { get; }

    public int TokenCount { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}

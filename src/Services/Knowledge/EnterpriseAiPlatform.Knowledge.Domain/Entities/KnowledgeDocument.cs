using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Domain.Entities;

public sealed record ChunkId(Guid Value)
{
    public static ChunkId New() => new(Guid.NewGuid());
}

public sealed class KnowledgeChunk : Entity<ChunkId>
{
    public Guid DocumentId { get; }
    public int ChunkIndex { get; }
    public string TextContent { get; }
    public float[] VectorEmbedding { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public KnowledgeChunk(
        ChunkId id,
        Guid documentId,
        int chunkIndex,
        string textContent,
        float[] vectorEmbedding,
        IDictionary<string, string>? metadata = null)
        : base(id)
    {
        DocumentId = documentId;
        ChunkIndex = chunkIndex;
        TextContent = textContent;
        VectorEmbedding = vectorEmbedding;
        Metadata = (metadata ?? new Dictionary<string, string>()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();
    }
}

public sealed class KnowledgeDocument : AggregateRoot<Guid>
{
    private readonly List<KnowledgeChunk> _chunks = new();

    public TenantId TenantId { get; }
    public string Title { get; }
    public string SourceUri { get; }
    public IReadOnlyList<KnowledgeChunk> Chunks => _chunks.AsReadOnly();
    public DateTimeOffset CreatedAt { get; }

    public KnowledgeDocument(Guid id, TenantId tenantId, string title, string sourceUri)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        SourceUri = sourceUri;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AddChunk(KnowledgeChunk chunk)
    {
        _chunks.Add(chunk);
    }
}

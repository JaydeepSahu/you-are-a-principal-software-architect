namespace EnterpriseAiPlatform.Knowledge.Domain;

public sealed class KnowledgeDocument : SharedKernel.AggregateRoot<KnowledgeDocumentId>
{
    private readonly List<KnowledgeDocumentVersion> _versions = [];
    private readonly List<KnowledgeChunk> _chunks = [];

    public KnowledgeDocument(
        KnowledgeDocumentId id,
        SharedKernel.TenantId tenantId,
        KnowledgeSource source,
        KnowledgeDocumentMetadata metadata,
        string contentHash,
        DateTimeOffset indexedAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);

        TenantId = tenantId;
        Source = source;
        Metadata = metadata;
        CurrentContentHash = contentHash;
        CreatedAtUtc = indexedAtUtc;
        UpdatedAtUtc = indexedAtUtc;
        _versions.Add(new KnowledgeDocumentVersion(1, contentHash, indexedAtUtc));
    }

    public SharedKernel.TenantId TenantId { get; }

    public KnowledgeSource Source { get; private set; }

    public KnowledgeDocumentMetadata Metadata { get; private set; }

    public string CurrentContentHash { get; private set; }

    public int CurrentVersion => _versions[^1].VersionNumber;

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public IReadOnlyList<KnowledgeDocumentVersion> Versions => _versions;

    public IReadOnlyList<KnowledgeChunk> Chunks => _chunks;

    public bool ApplyIndex(
        KnowledgeSource source,
        KnowledgeDocumentMetadata metadata,
        string contentHash,
        IReadOnlyList<KnowledgeChunk> chunks,
        DateTimeOffset indexedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);
        ArgumentNullException.ThrowIfNull(chunks);

        Source = source;
        Metadata = metadata;
        UpdatedAtUtc = indexedAtUtc;

        if (CurrentContentHash == contentHash)
        {
            ReplaceChunks(chunks);
            return false;
        }

        CurrentContentHash = contentHash;
        _versions.Add(new KnowledgeDocumentVersion(CurrentVersion + 1, contentHash, indexedAtUtc));
        ReplaceChunks(chunks);
        RaiseDomainEvent(new KnowledgeDocumentIndexedDomainEvent(Id, TenantId, CurrentVersion));
        return true;
    }

    private void ReplaceChunks(IReadOnlyList<KnowledgeChunk> chunks)
    {
        _chunks.Clear();
        _chunks.AddRange(chunks);
    }
}

public sealed record KnowledgeDocumentIndexedDomainEvent(
    KnowledgeDocumentId DocumentId,
    SharedKernel.TenantId TenantId,
    int Version) : SharedKernel.DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);

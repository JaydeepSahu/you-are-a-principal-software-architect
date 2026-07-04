namespace EnterpriseAiPlatform.Knowledge.Contracts.Responses;

public sealed record IngestKnowledgeResponse(
    IReadOnlyList<IngestedDocumentResponse> Documents,
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount);

public sealed record IngestedDocumentResponse(
    Guid DocumentId,
    string SourceType,
    string ExternalId,
    string Title,
    int Version,
    bool Changed,
    int ChunkCount,
    DateTimeOffset IndexedAtUtc);

public sealed record KnowledgeDocumentResponse(
    Guid DocumentId,
    string SourceType,
    string ExternalId,
    Uri? Uri,
    string Title,
    string? ContentType,
    string Language,
    int Version,
    string ContentHash,
    int ChunkCount,
    IReadOnlyDictionary<string, string> Metadata,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record KnowledgeDocumentVersionResponse(
    int Version,
    string ContentHash,
    DateTimeOffset IndexedAtUtc);

public sealed record SearchKnowledgeResponse(
    string Query,
    IReadOnlyList<SearchHitResponse> Hits);

public sealed record SearchHitResponse(
    Guid DocumentId,
    Guid ChunkId,
    string SourceType,
    string ExternalId,
    string Title,
    int Version,
    int ChunkOrdinal,
    string Text,
    double Score,
    IReadOnlyDictionary<string, string> Metadata);

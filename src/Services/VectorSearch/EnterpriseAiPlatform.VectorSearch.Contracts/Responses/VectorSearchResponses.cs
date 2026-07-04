namespace EnterpriseAiPlatform.VectorSearch.Contracts.Responses;

public sealed record UpsertVectorDocumentsResponse(
    int UpsertedCount,
    IReadOnlyList<Guid> DocumentIds,
    DateTimeOffset IndexedAtUtc);

public sealed record SearchVectorsResponse(
    string Query,
    string Mode,
    bool CacheHit,
    double ElapsedMilliseconds,
    IReadOnlyList<VectorSearchHitResponse> Hits);

public sealed record VectorSearchHitResponse(
    Guid DocumentId,
    string ExternalId,
    string Content,
    double Score,
    double SemanticScore,
    double KeywordScore,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record VectorSearchProgressResponse(
    string Provider,
    int CachedQueries,
    int IndexedDocuments,
    DateTimeOffset CheckedAtUtc);

using EnterpriseAiPlatform.SharedKernel;
using EnterpriseAiPlatform.VectorSearch.Domain;

namespace EnterpriseAiPlatform.VectorSearch.Application.Abstractions;

public interface IVectorStore
{
    VectorSearchProvider Provider { get; }

    Task UpsertAsync(IReadOnlyList<VectorRecord> records, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorSearchCandidate>> SearchAsync(
        TenantId tenantId,
        double[] queryEmbedding,
        IReadOnlyDictionary<string, string> metadataFilters,
        int candidateCount,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(TenantId tenantId, CancellationToken cancellationToken = default);
}

public interface IEmbeddingGenerator
{
    double[] Generate(string text);
}

public interface ISearchResultCache
{
    bool TryGet(string key, out CachedVectorSearchResult result);

    void Store(string key, CachedVectorSearchResult result, TimeSpan ttl);

    int Count { get; }
}

public interface IVectorSearchTelemetry
{
    DateTimeOffset LastIndexedAtUtc { get; }

    void MarkIndexed(DateTimeOffset indexedAtUtc);
}

public sealed record VectorSearchCandidate(
    VectorRecord Record,
    double SemanticScore);

public sealed record VectorSearchHit(
    VectorRecord Record,
    double Score,
    double SemanticScore,
    double KeywordScore);

public sealed record CachedVectorSearchResult(IReadOnlyList<VectorSearchHit> Hits, DateTimeOffset CachedAtUtc);

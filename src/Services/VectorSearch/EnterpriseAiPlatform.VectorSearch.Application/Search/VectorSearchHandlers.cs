using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using EnterpriseAiPlatform.VectorSearch.Application.Abstractions;
using EnterpriseAiPlatform.VectorSearch.Contracts.Responses;
using EnterpriseAiPlatform.VectorSearch.Domain;

namespace EnterpriseAiPlatform.VectorSearch.Application.Search;

public sealed class UpsertVectorDocumentsHandler(
    IVectorStore vectorStore,
    IEmbeddingGenerator embeddingGenerator,
    IVectorSearchTelemetry telemetry,
    IRequestContextAccessor requestContext)
    : ICommandHandler<UpsertVectorDocumentsCommand, UpsertVectorDocumentsResponse>
{
    public async Task<Result<UpsertVectorDocumentsResponse>> Handle(
        UpsertVectorDocumentsCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var records = command.Request.Documents
            .Select(document => new VectorRecord(
                VectorDocumentId.New(),
                tenantId,
                document.ExternalId,
                document.Content,
                document.Embedding ?? embeddingGenerator.Generate(document.Content),
                document.Metadata))
            .ToList();

        await vectorStore.UpsertAsync(records, cancellationToken);
        var indexedAtUtc = DateTimeOffset.UtcNow;
        telemetry.MarkIndexed(indexedAtUtc);

        return Result.Success(new UpsertVectorDocumentsResponse(
            records.Count,
            records.Select(record => record.Id.Value).ToList(),
            indexedAtUtc));
    }
}

public sealed class SearchVectorsHandler(
    IVectorStore vectorStore,
    IEmbeddingGenerator embeddingGenerator,
    ISearchResultCache cache,
    IRequestContextAccessor requestContext)
    : IQueryHandler<SearchVectorsQuery, SearchVectorsResponse>
{
    public async Task<Result<SearchVectorsResponse>> Handle(
        SearchVectorsQuery query,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var tenantId = requestContext.Current.TenantId;
        var filters = query.Request.MetadataFilters ?? new Dictionary<string, string>();
        var queryEmbedding = query.Request.QueryEmbedding ?? embeddingGenerator.Generate(query.Request.Query);
        var cacheKey = CreateCacheKey(tenantId, query.Request.Query, query.Request.Mode, query.Request.TopK, filters, queryEmbedding);

        if (query.Request.UseCache && cache.TryGet(cacheKey, out var cached))
        {
            stopwatch.Stop();
            return Result.Success(Map(query.Request.Query, query.Request.Mode, true, stopwatch.Elapsed, cached.Hits));
        }

        var candidateCount = Math.Min(Math.Max(query.Request.TopK * 5, 25), 500);
        var candidates = await vectorStore.SearchAsync(tenantId, queryEmbedding, filters, candidateCount, cancellationToken);
        var hits = candidates
            .Select(candidate =>
            {
                var keywordScore = query.Request.Mode.Equals("Hybrid", StringComparison.OrdinalIgnoreCase)
                    ? KeywordScore(query.Request.Query, candidate.Record.Content, candidate.Record.Metadata)
                    : 0d;
                var score = query.Request.Mode.Equals("Hybrid", StringComparison.OrdinalIgnoreCase)
                    ? candidate.SemanticScore * 0.72 + keywordScore * 0.28
                    : candidate.SemanticScore;
                return new VectorSearchHit(candidate.Record, score, candidate.SemanticScore, keywordScore);
            })
            .OrderByDescending(hit => query.Request.Rerank ? RerankScore(query.Request.Query, hit) : hit.Score)
            .ThenBy(hit => hit.Record.ExternalId)
            .Take(query.Request.TopK)
            .ToList();

        cache.Store(cacheKey, new CachedVectorSearchResult(hits, DateTimeOffset.UtcNow), TimeSpan.FromMinutes(5));
        stopwatch.Stop();

        return Result.Success(Map(query.Request.Query, query.Request.Mode, false, stopwatch.Elapsed, hits));
    }

    private static SearchVectorsResponse Map(string query, string mode, bool cacheHit, TimeSpan elapsed, IReadOnlyList<VectorSearchHit> hits)
        => new(
            query,
            mode,
            cacheHit,
            elapsed.TotalMilliseconds,
            hits.Select(hit => new VectorSearchHitResponse(
                    hit.Record.Id.Value,
                    hit.Record.ExternalId,
                    hit.Record.Content,
                    Math.Round(hit.Score, 4),
                    Math.Round(hit.SemanticScore, 4),
                    Math.Round(hit.KeywordScore, 4),
                    hit.Record.Metadata))
                .ToList());

    private static double RerankScore(string query, VectorSearchHit hit)
        => hit.Score + ExactPhraseBoost(query, hit.Record.Content) + MetadataBoost(query, hit.Record.Metadata);

    private static double KeywordScore(string query, string content, IReadOnlyDictionary<string, string> metadata)
    {
        var queryTerms = Terms(query).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (queryTerms.Count == 0)
        {
            return 0;
        }

        var contentTerms = Terms(content)
            .Concat(metadata.SelectMany(item => Terms(item.Value)))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return queryTerms.Count(term => contentTerms.Contains(term)) / (double)queryTerms.Count;
    }

    private static double ExactPhraseBoost(string query, string content)
        => content.Contains(query, StringComparison.OrdinalIgnoreCase) ? 0.08 : 0;

    private static double MetadataBoost(string query, IReadOnlyDictionary<string, string> metadata)
        => metadata.Any(item => item.Value.Contains(query, StringComparison.OrdinalIgnoreCase)) ? 0.04 : 0;

    private static IEnumerable<string> Terms(string text)
        => Regex.Matches(text.ToLowerInvariant(), @"[\p{L}\p{N}_]+")
            .Select(match => match.Value)
            .Where(term => term.Length > 1);

    private static string CreateCacheKey(
        TenantId tenantId,
        string query,
        string mode,
        int topK,
        IReadOnlyDictionary<string, string> filters,
        double[] embedding)
    {
        var filterText = string.Join('|', filters.OrderBy(item => item.Key).Select(item => $"{item.Key}={item.Value}"));
        var embeddingFingerprint = string.Join(',', embedding.Take(8).Select(value => value.ToString("F4", CultureInfo.InvariantCulture)));
        var raw = $"{tenantId}:{query}:{mode}:{topK}:{filterText}:{embeddingFingerprint}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}

public sealed class GetVectorSearchProgressHandler(
    IVectorStore vectorStore,
    ISearchResultCache cache,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetVectorSearchProgressQuery, VectorSearchProgressResponse>
{
    public async Task<Result<VectorSearchProgressResponse>> Handle(
        GetVectorSearchProgressQuery query,
        CancellationToken cancellationToken)
    {
        var indexedDocuments = await vectorStore.CountAsync(requestContext.Current.TenantId, cancellationToken);
        return Result.Success(new VectorSearchProgressResponse(
            vectorStore.Provider.ToString(),
            cache.Count,
            indexedDocuments,
            DateTimeOffset.UtcNow));
    }
}

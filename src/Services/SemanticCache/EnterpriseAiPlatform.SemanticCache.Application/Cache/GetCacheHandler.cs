using System.Diagnostics;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Contracts.Requests;
using EnterpriseAiPlatform.SemanticCache.Contracts.Responses;
using EnterpriseAiPlatform.SemanticCache.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.SemanticCache.Application.Cache;

public sealed record GetCacheQuery(GetCacheRequest Request) : IQuery<GetCacheResponse>;

public sealed class GetCacheHandler(
    ISemanticCacheStore store,
    ISemanticCacheMetrics metrics,
    IEmbeddingGenerator embeddingGenerator,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetCacheQuery, GetCacheResponse>
{
    public async Task<Result<GetCacheResponse>> Handle(GetCacheQuery query, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var tenantId = requestContext.Current.TenantId;
        var req = query.Request;

        if (!Enum.TryParse<SemanticCacheType>(req.CacheType, true, out var cacheType))
            return Result.Failure<GetCacheResponse>(SemanticCacheErrors.InvalidCacheType(req.CacheType));

        var version = new SemanticCacheVersion(req.Version);
        var embedding = req.QueryEmbedding ?? embeddingGenerator.Generate(req.Key);
        var minSimilarity = req.MinSimilarity ?? 0.92;

        var result = await store.GetAsync(tenantId, cacheType, version, req.Key, embedding, minSimilarity, cancellationToken);
        stopwatch.Stop();

        if (result is null)
        {
            metrics.RecordCacheMiss(tenantId, cacheType, version.Value);
            metrics.RecordCacheLatency(tenantId, cacheType, version.Value, stopwatch.Elapsed, hit: false);
            return Result.Success(new GetCacheResponse(null, null, null, false, null, null));
        }

        metrics.RecordCacheHit(tenantId, cacheType, version.Value, result.SimilarityScore);
        metrics.RecordCacheLatency(tenantId, cacheType, version.Value, stopwatch.Elapsed, hit: true);

        return Result.Success(new GetCacheResponse(
            result.KeyHash,
            result.Value,
            result.SimilarityScore,
            true,
            result.CachedAtUtc,
            result.ExpiresAtUtc));
    }
}

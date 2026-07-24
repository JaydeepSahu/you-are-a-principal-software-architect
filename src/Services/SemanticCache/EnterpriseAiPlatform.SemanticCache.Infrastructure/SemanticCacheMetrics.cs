using System.Diagnostics.Metrics;
using EnterpriseAiPlatform.SemanticCache.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Domain;

namespace EnterpriseAiPlatform.SemanticCache.Infrastructure;

public sealed class SemanticCacheMetrics : ISemanticCacheMetrics
{
    public const string MeterName = "EnterpriseAiPlatform.SemanticCache";

    private static readonly Meter Meter = new(MeterName, "1.0.0");

    private static readonly Counter<long> CacheHits = Meter.CreateCounter<long>(
        "semantic_cache_hits_total",
        description: "Total semantic cache hits.");

    private static readonly Counter<long> CacheMisses = Meter.CreateCounter<long>(
        "semantic_cache_misses_total",
        description: "Total semantic cache misses.");

    private static readonly Counter<long> CacheSets = Meter.CreateCounter<long>(
        "semantic_cache_sets_total",
        description: "Total semantic cache set operations.");

    private static readonly Counter<long> CacheInvalidations = Meter.CreateCounter<long>(
        "semantic_cache_invalidations_total",
        description: "Total semantic cache invalidation operations.");

    private static readonly Counter<long> CacheEvictions = Meter.CreateCounter<long>(
        "semantic_cache_evictions_total",
        description: "Total semantic cache evictions.");

    private static readonly Histogram<double> CacheLatency = Meter.CreateHistogram<double>(
        "semantic_cache_latency_ms",
        unit: "ms",
        description: "Semantic cache operation latency in milliseconds.");

    private static readonly Histogram<double> SimilarityScore = Meter.CreateHistogram<double>(
        "semantic_cache_similarity_score",
        description: "Distribution of similarity scores for cache hits.");

    public void RecordCacheHit(TenantId tenantId, SemanticCacheType cacheType, string version, double similarityScore)
    {
        var tags = Tags(tenantId, cacheType, version);
        CacheHits.Add(1, tags);
        SimilarityScore.Record(similarityScore, tags);
    }

    public void RecordCacheMiss(TenantId tenantId, SemanticCacheType cacheType, string version)
    {
        CacheMisses.Add(1, Tags(tenantId, cacheType, version));
    }

    public void RecordCacheSet(TenantId tenantId, SemanticCacheType cacheType, string version)
    {
        CacheSets.Add(1, Tags(tenantId, cacheType, version));
    }

    public void RecordCacheInvalidation(TenantId tenantId, SemanticCacheType cacheType, string version, int keysInvalidated)
    {
        CacheInvalidations.Add(keysInvalidated, Tags(tenantId, cacheType, version));
    }

    public void RecordCacheEviction(TenantId tenantId, SemanticCacheType cacheType, string version)
    {
        CacheEvictions.Add(1, Tags(tenantId, cacheType, version));
    }

    public void RecordCacheLatency(TenantId tenantId, SemanticCacheType cacheType, string version, TimeSpan elapsed, bool hit)
    {
        var tags = [.. Tags(tenantId, cacheType, version), new KeyValuePair<string, object?>("hit", hit)];
        CacheLatency.Record(elapsed.TotalMilliseconds, tags);
    }

    private static KeyValuePair<string, object?>[] Tags(TenantId tenantId, SemanticCacheType cacheType, string version) =>
    [
        new("tenant_id", tenantId.Value.ToString("D")),
        new("cache_type", cacheType.ToString().ToLowerInvariant()),
        new("cache_version", version)
    ];
}

using EnterpriseAiPlatform.SemanticCache.Domain;

namespace EnterpriseAiPlatform.SemanticCache.Application.Abstractions;

public interface ISemanticCacheStore
{
    Task SetAsync(
        SharedKernel.TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        string key,
        double[] embedding,
        string? value,
        TimeSpan ttl,
        IReadOnlySet<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task<SemanticCacheHitResult?> GetAsync(
        SharedKernel.TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        string key,
        double[] queryEmbedding,
        double minSimilarity,
        CancellationToken cancellationToken = default);

    Task<int> InvalidateAsync(
        SharedKernel.TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion? version,
        string? key,
        string? tag,
        CancellationToken cancellationToken = default);

    Task<SemanticCacheStats> GetStatsAsync(
        SharedKernel.TenantId tenantId,
        SemanticCacheType? cacheType,
        SemanticCacheVersion? version,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CacheKeyInfo>> ListKeysAsync(
        SharedKernel.TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<long> CountKeysAsync(
        SharedKernel.TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        CancellationToken cancellationToken = default);
}

public sealed record SemanticCacheHitResult(
    string KeyHash,
    string Value,
    double SimilarityScore,
    DateTimeOffset CachedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    long HitCount);

public sealed record CacheKeyInfo(
    string KeyHash,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    long HitCount,
    IReadOnlyList<string> Tags);

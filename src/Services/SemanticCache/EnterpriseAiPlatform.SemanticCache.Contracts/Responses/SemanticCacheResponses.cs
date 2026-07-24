namespace EnterpriseAiPlatform.SemanticCache.Contracts.Responses;

public sealed record SetCacheResponse(
    string KeyHash,
    DateTimeOffset ExpiresAtUtc);

public sealed record GetCacheResponse(
    string? KeyHash,
    string? Value,
    double? SimilarityScore,
    bool CacheHit,
    DateTimeOffset? CachedAtUtc,
    DateTimeOffset? ExpiresAtUtc);

public sealed record InvalidateCacheResponse(
    int KeysInvalidated,
    int TagsInvalidated);

public sealed record GetCacheStatsResponse(
    long TotalEntries,
    long TotalHits,
    long TotalMisses,
    double HitRatePercent,
    long TotalEvictions,
    long TotalKeys);

public sealed record ListCacheKeysResponse(
    IReadOnlyList<CacheKeyInfo> Keys,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record CacheKeyInfo(
    string KeyHash,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    long HitCount,
    IReadOnlyList<string> Tags);

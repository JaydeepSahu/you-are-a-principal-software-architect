namespace EnterpriseAiPlatform.SemanticCache.Contracts.Requests;

public sealed record SetCacheRequest(
    string CacheType,
    string Version,
    string Key,
    string? Value,
    double[]? Embedding,
    double? SimilarityThreshold,
    TimeSpan? TtlSeconds,
    IReadOnlyList<string>? Tags);

public sealed record GetCacheRequest(
    string CacheType,
    string Version,
    string Key,
    double[]? QueryEmbedding,
    double? MinSimilarity);

public sealed record InvalidateCacheRequest(
    string CacheType,
    string? Version,
    string? Key,
    string? Tag);

public sealed record GetCacheStatsRequest(
    string? CacheType,
    string? Version);

public sealed record ListCacheKeysRequest(
    string CacheType,
    string Version,
    int Page = 1,
    int PageSize = 50);

using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Caching.Abstractions;

public sealed record CachedResponseEntry(
    string OriginalPrompt,
    string ModelId,
    string ResponsePayload,
    DateTimeOffset CreatedAtUtc,
    long SavingsCostMs);

public sealed record SemanticCacheResult(
    bool IsHit,
    double CosineSimilarity,
    CachedResponseEntry? Entry);

public interface ISemanticCache
{
    Task<Result<SemanticCacheResult>> GetCachedResponseAsync(
        TenantId tenantId,
        string prompt,
        double minSimilarityThreshold = 0.95,
        CancellationToken cancellationToken = default);

    Task CacheResponseAsync(
        TenantId tenantId,
        string prompt,
        string responsePayload,
        string modelId,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);
}

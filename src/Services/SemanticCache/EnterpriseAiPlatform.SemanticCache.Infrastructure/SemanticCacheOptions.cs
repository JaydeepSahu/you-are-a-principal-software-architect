namespace EnterpriseAiPlatform.SemanticCache.Infrastructure;

public sealed class SemanticCacheOptions
{
    public string? RedisConnectionString { get; set; }
    public string KeyPrefix { get; set; } = "semcache";
    public int MaxSimilarityCandidates { get; set; } = 50;
    public int EmbeddingDimensions { get; set; } = 256;
    public bool EnableMetrics { get; set; } = true;
}

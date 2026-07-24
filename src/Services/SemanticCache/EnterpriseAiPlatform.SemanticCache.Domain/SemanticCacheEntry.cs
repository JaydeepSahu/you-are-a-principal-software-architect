namespace EnterpriseAiPlatform.SemanticCache.Domain;

public sealed class SemanticCacheEntry : SharedKernel.Entity<SemanticCacheEntryId>
{
    public SemanticCacheEntry(
        SemanticCacheEntryId id,
        SharedKernel.TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        string keyHash,
        double[] embedding,
        string? cachedValue,
        TimeSpan ttl,
        IReadOnlySet<string>? tags = null)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyHash);
        if (embedding.Length == 0) throw new ArgumentException("Embedding cannot be empty.", nameof(embedding));

        TenantId = tenantId;
        CacheType = cacheType;
        Version = version;
        KeyHash = keyHash;
        Embedding = embedding;
        CachedValue = cachedValue;
        Ttl = ttl;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        ExpiresAtUtc = CreatedAtUtc.Add(ttl);
        HitCount = 0;
        Tags = tags is null ? [] : new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase);
    }

    public SharedKernel.TenantId TenantId { get; }
    public SemanticCacheType CacheType { get; }
    public SemanticCacheVersion Version { get; }
    public string KeyHash { get; }
    public double[] Embedding { get; }
    public string? CachedValue { get; }
    public TimeSpan Ttl { get; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public long HitCount { get; private set; }
    public IReadOnlySet<string> Tags { get; }

    public void IncrementHit() => HitCount++;

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;

    public double CosineSimilarityTo(double[] other)
    {
        if (other.Length != Embedding.Length) return 0;
        var dot = 0d;
        for (var i = 0; i < Embedding.Length; i++) dot += Embedding[i] * other[i];
        return Math.Max(0, dot);
    }
}

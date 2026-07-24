using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Domain;
using SharedKernel = EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.SemanticCache.Infrastructure;

public sealed class InMemorySemanticCacheStore : ISemanticCacheStore
{
    private readonly ConcurrentDictionary<string, InMemoryEntry> _store = new();
    private long _hits, _misses, _evictions, _sets;

    public Task SetAsync(
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        string key,
        double[] embedding,
        string? value,
        TimeSpan ttl,
        IReadOnlySet<string>? tags,
        CancellationToken cancellationToken = default)
    {
        var keyHash = ComputeKeyHash(tenantId, cacheType, version, key);
        var entryKey = CompositeKey(tenantId, cacheType, version, keyHash);
        var entry = new InMemoryEntry(
            keyHash, tenantId, cacheType, version, embedding, value, ttl, tags ?? [],
            DateTimeOffset.UtcNow, 0);
        _store[keyHash] = entry;
        Interlocked.Increment(ref _sets);
        return Task.CompletedTask;
    }

    public Task<SemanticCacheHitResult?> GetAsync(
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        string key,
        double[] queryEmbedding,
        double minSimilarity,
        CancellationToken cancellationToken = default)
    {
        var keyHash = ComputeKeyHash(tenantId, cacheType, version, key);
        if (!_store.TryGetValue(keyHash, out var entry) || entry.IsExpired)
        {
            Interlocked.Increment(ref _misses);
            _store.TryRemove(keyHash, out _);
            return Task.FromResult<SemanticCacheHitResult?>(null);
        }

        var similarity = CosineSimilarity(queryEmbedding, entry.Embedding);
        if (similarity < minSimilarity)
        {
            Interlocked.Increment(ref _misses);
            return Task.FromResult<SemanticCacheHitResult?>(null);
        }

        entry.IncrementHit();
        Interlocked.Increment(ref _hits);

        return Task.FromResult<SemanticCacheHitResult?>(new(
            entry.KeyHash,
            entry.Value ?? string.Empty,
            similarity,
            entry.CreatedAtUtc,
            entry.ExpiresAtUtc,
            entry.HitCount));
    }

    public Task<int> InvalidateAsync(
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion? version,
        string? key,
        string? tag,
        CancellationToken cancellationToken = default)
    {
        int count = 0;
        var keys = _store.Keys.ToList();

        foreach (var k in keys)
        {
            if (!_store.TryGetValue(k, out var entry))
                continue;

            if (entry.TenantId != tenantId)
                continue;
            if (entry.CacheType != cacheType)
                continue;
            if (version is not null && entry.Version != version)
                continue;

            bool remove = false;
            if (key is not null)
                remove = ComputeKeyHash(tenantId, cacheType, version ?? entry.Version, key) == k;
            else if (tag is not null)
                remove = entry.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
            else
                remove = true;

            if (remove && _store.TryRemove(k, out _))
            {
                Interlocked.Increment(ref _evictions);
                count++;
            }
        }

        return Task.FromResult(count);
    }

    public Task<SemanticCacheStats> GetStatsAsync(
        TenantId tenantId,
        SemanticCacheType? cacheType,
        SemanticCacheVersion? version,
        CancellationToken cancellationToken = default)
    {
        var count = _store.LongCount(e =>
            e.Value.TenantId == tenantId
            && (!cacheType.HasValue || e.Value.CacheType == cacheType)
            && (!version.HasValue || e.Value.Version == version)
            && !e.Value.IsExpired);

        var total = _hits + _misses;
        var hitRate = total > 0 ? (double)_hits / total * 100 : 0;

        return Task.FromResult(new SemanticCacheStats(count, _hits, _misses, hitRate, _evictions, 0));
    }

    public Task<IReadOnlyList<CacheKeyInfo>> ListKeysAsync(
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var pageSize2 = Math.Clamp(pageSize, 1, 200);
        var page2 = Math.Max(page, 1);
        var skip = (page2 - 1) * pageSize2;

        var results = _store.Values
            .Where(e => e.TenantId == tenantId && e.CacheType == cacheType && e.Version == version && !e.IsExpired)
            .OrderByDescending(e => e.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize2)
            .Select(e => new CacheKeyInfo(e.KeyHash, e.CreatedAtUtc, e.ExpiresAtUtc, e.HitCount, e.Tags))
            .ToList();

        return Task.FromResult<IReadOnlyList<CacheKeyInfo>>(results);
    }

    public Task<long> CountKeysAsync(
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        CancellationToken cancellationToken = default)
    {
        var count = _store.Values
            .LongCount(e => e.TenantId == tenantId && e.CacheType == cacheType && e.Version == version && !e.IsExpired);
        return Task.FromResult(count);
    }

    private static string CompositeKey(TenantId tenantId, SemanticCacheType cacheType, SemanticCacheVersion version, string keyHash)
        => $"{tenantId.Value:N}:{cacheType}:{version}:{keyHash}";

    private static string ComputeKeyHash(TenantId tenantId, SemanticCacheType cacheType, SemanticCacheVersion version, string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{tenantId}:{cacheType}:{version}:{key}"));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static double CosineSimilarity(double[] a, double[] b)
    {
        var len = Math.Min(a.Length, b.Length);
        var dot = 0d;
        for (var i = 0; i < len; i++) dot += a[i] * b[i];
        return Math.Max(0, dot);
    }

    private sealed class InMemoryEntry(
        string keyHash,
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        double[] embedding,
        string? value,
        TimeSpan ttl,
        IReadOnlySet<string> tags,
        DateTimeOffset createdAtUtc,
        long hitCount)
    {
        public string KeyHash { get; } = keyHash;
        public TenantId TenantId { get; } = tenantId;
        public SemanticCacheType CacheType { get; } = cacheType;
        public SemanticCacheVersion Version { get; } = version;
        public double[] Embedding { get; } = embedding;
        public string? Value { get; } = value;
        public TimeSpan Ttl { get; } = ttl;
        public IReadOnlySet<string> Tags { get; } = tags;
        public DateTimeOffset CreatedAtUtc { get; } = createdAtUtc;
        public DateTimeOffset ExpiresAtUtc { get; } = createdAtUtc.Add(ttl);
        public long HitCount { get; private set; } = hitCount;

        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;
        public void IncrementHit() => HitCount++;
    }
}

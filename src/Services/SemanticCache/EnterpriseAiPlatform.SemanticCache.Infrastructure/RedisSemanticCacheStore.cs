using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EnterpriseAiPlatform.SemanticCache.Infrastructure;

public sealed class RedisSemanticCacheStore : ISemanticCacheStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly SemanticCacheOptions _options;
    private readonly int _dimensions;
    private readonly IRequestContextAccessor _requestContext;

    private static readonly string GetAndHitScript = """
        local key = KEYS[1]
        local entry = redis.call('HGETALL', key)
        if #entry == 0 then
            return nil
        end
        redis.call('HINCRBY', key, 'hit_count', 1)
        return entry
        """;

    private static readonly string ScanIndexScript = """
        local indexKey = KEYS[1]
        local startScore = tonumber(ARGV[1]) or 0
        local count = tonumber(ARGV[2]) or 50
        local results = redis.call('ZREVRANGEBYSCORE', indexKey, '+inf', startScore, 'LIMIT', '0', count)
        return results
        """;

    public RedisSemanticCacheStore(
        IConnectionMultiplexer redis,
        IOptions<SemanticCacheOptions> options,
        IRequestContextAccessor requestContext)
    {
        _redis = redis;
        _options = options.Value;
        _dimensions = _options.EmbeddingDimensions;
        _requestContext = requestContext;
    }

    public async Task SetAsync(
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
        var db = _redis.GetDatabase();
        var keyHash = ComputeKeyHash(tenantId, cacheType, version, key);
        var entryKey = EntryKey(tenantId, cacheType, version, keyHash);
        var indexKey = IndexKey(tenantId, cacheType, version);
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(ttl);
        var embeddingStr = EncodeEmbedding(embedding);

        var batch = db.CreateBatch();
        var tasks = new List<Task>();

        var hashEntries = new HashEntry[]
        {
            new("value", value ?? string.Empty),
            new("embedding", embeddingStr),
            new("created_at", now.ToUnixTimeMilliseconds()),
            new("expires_at", expiresAt.ToUnixTimeMilliseconds()),
            new("hit_count", 0),
            new("dot_product_seed", embedding[0].ToString("R", CultureInfo.InvariantCulture)),
        };
        tasks.Add(batch.HashSetAsync(entryKey, hashEntries));
        tasks.Add(batch.KeyExpireAsync(entryKey, ttl));

        var norm = Math.Sqrt(embedding.Sum(v => v * v));
        var normalizedSeed = norm > 0 ? embedding[0] / norm : 0;
        tasks.Add(batch.SortedSetAddAsync(indexKey, keyHash, normalizedSeed));

        if (tags is { Count: > 0 })
        {
            foreach (var tag in tags)
            {
                var tagKey = TagKey(tenantId, cacheType, version, tag);
                tasks.Add(batch.SetAddAsync(tagKey, keyHash));
                tasks.Add(batch.KeyExpireAsync(tagKey, ttl));
            }
        }

        batch.Execute();
        await Task.WhenAll(tasks);

        await IncrementCounterAsync(db, tenantId, cacheType, version, "sets", 1);
    }

    public async Task<SemanticCacheHitResult?> GetAsync(
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        string key,
        double[] queryEmbedding,
        double minSimilarity,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var indexKey = IndexKey(tenantId, cacheType, version);
        var maxCandidates = _options.MaxSimilarityCandidates;

        var candidates = await db.ScriptEvaluateAsync(
            ScanIndexScript,
            [indexKey],
            ["0", maxCandidates.ToString(CultureInfo.InvariantCulture)]);

        var candidateHashes = ((RedisResult[])candidates!)
            .Select(r => (string)r!)
            .ToList();

        if (candidateHashes.Count == 0)
            return null;

        double bestSimilarity = -1;
        string? bestKeyHash = null;
        HashEntry[]? bestEntry = null;

        foreach (var candidateHash in candidateHashes)
        {
            var entryKey = EntryKey(tenantId, cacheType, version, candidateHash);
            var entry = await db.HashGetAllAsync(entryKey);

            if (entry is null or { Length: 0 })
                continue;

            var dict = entry.ToDictionary(
                r => r.Name.ToString(),
                r => r.Value.ToString());

            if (!dict.TryGetValue("expires_at", out var expiresStr)
                || !long.TryParse(expiresStr, out var expiresMs))
                continue;

            if (DateTimeOffset.UtcNow >= DateTimeOffset.FromUnixTimeMilliseconds(expiresMs))
            {
                _ = EvictExpiredEntryAsync(db, entryKey, indexKey, candidateHash);
                continue;
            }

            if (!dict.TryGetValue("embedding", out var embeddingStr))
                continue;

            var embedding = DecodeEmbedding(embeddingStr);
            var similarity = CosineSimilarity(queryEmbedding, embedding);

            if (similarity >= minSimilarity && similarity > bestSimilarity)
            {
                bestSimilarity = similarity;
                bestKeyHash = candidateHash;
                bestEntry = entry;
            }
        }

        if (bestKeyHash is null || bestEntry is null)
        {
            await IncrementCounterAsync(db, tenantId, cacheType, version, "misses", 1);
            return null;
        }

        var entryDict = bestEntry.ToDictionary(
            r => r.Name.ToString(),
            r => r.Value.ToString());

        entryDict.TryGetValue("created_at", out var createdStr);
        entryDict.TryGetValue("value", out var value);

        await db.ScriptEvaluateAsync(
            GetAndHitScript,
            [EntryKey(tenantId, cacheType, version, bestKeyHash)],
            []);

        await IncrementCounterAsync(db, tenantId, cacheType, version, "hits", 1);

        var clampedSimilarity = Math.Max(0, bestSimilarity);

        return new SemanticCacheHitResult(
            bestKeyHash,
            value ?? string.Empty,
            clampedSimilarity,
            long.TryParse(createdStr, out var ct) ? DateTimeOffset.FromUnixTimeMilliseconds(ct) : DateTimeOffset.UtcNow,
            long.TryParse(entryDict.GetValueOrDefault("expires_at"), out var et) ? DateTimeOffset.FromUnixTimeMilliseconds(et) : DateTimeOffset.UtcNow,
            0);
    }

    public async Task<int> InvalidateAsync(
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion? version,
        string? key,
        string? tag,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var count = 0;
        var effectiveVersion = version ?? SemanticCacheVersion.Default;

        if (key is not null)
        {
            var keyHash = ComputeKeyHash(tenantId, cacheType, effectiveVersion, key);
            var entryKey = EntryKey(tenantId, cacheType, effectiveVersion, keyHash);
            var indexKey = IndexKey(tenantId, cacheType, effectiveVersion);

            if (await db.KeyDeleteAsync(entryKey))
                count++;
            await db.SortedSetRemoveAsync(indexKey, keyHash);

            return count;
        }

        if (tag is not null)
        {
            var tagKey = TagKey(tenantId, cacheType, effectiveVersion, tag);
            var members = await db.SetMembersAsync(tagKey);

            if (members.Length > 0)
            {
                var entryKeys = members
                    .Select(m => (RedisKey)EntryKey(tenantId, cacheType, effectiveVersion, m.ToString()))
                    .ToArray();
                var indexKey = IndexKey(tenantId, cacheType, effectiveVersion);

                count += (int)await db.KeyDeleteAsync(entryKeys);
                await db.KeyDeleteAsync(tagKey);

                var indexMembers = members.Select(m => (RedisValue)m.ToString()).ToArray();
                await db.SortedSetRemoveAsync(indexKey, indexMembers);
            }

            return count;
        }

        if (version is not null)
        {
            var indexKey = IndexKey(tenantId, cacheType, effectiveVersion);
            await db.KeyDeleteAsync(indexKey);

            var server = _redis.GetServers().First();
            var pattern = $"{_options.KeyPrefix}:{tenantId.Value:N}:{cacheType}:{effectiveVersion}:*";
            var keysToDelete = new List<RedisKey>();

            await foreach (var k in server.KeysAsync(pattern: pattern))
            {
                keysToDelete.Add(k);
            }

            if (keysToDelete.Count > 0)
                count += (int)await db.KeyDeleteAsync(keysToDelete.ToArray());

            return count;
        }

        var allVersions = new[] { "v1" };
        foreach (var v in allVersions)
        {
            var ver = new SemanticCacheVersion(v);
            var idxKey = IndexKey(tenantId, cacheType, ver);
            await db.KeyDeleteAsync(idxKey);

            var server2 = _redis.GetServers().First();
            var p = $"{_options.KeyPrefix}:{tenantId.Value:N}:{cacheType}:{v}:*";
            var kd = new List<RedisKey>();
            await foreach (var k in server2.KeysAsync(pattern: p))
                kd.Add(k);
            if (kd.Count > 0)
                count += (int)await db.KeyDeleteAsync(kd.ToArray());
        }

        return count;
    }

    public async Task<SemanticCacheStats> GetStatsAsync(
        TenantId tenantId,
        SemanticCacheType? cacheType,
        SemanticCacheVersion? version,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var types = cacheType.HasValue
            ? [cacheType.Value]
            : Enum.GetValues<SemanticCacheType>();

        long totalHits = 0, totalMisses = 0, totalEvictions = 0, totalEntries = 0;

        foreach (var ct in types)
        {
            var hits = await db.StringGetAsync(CounterKey(tenantId, ct, version, "hits"));
            var misses = await db.StringGetAsync(CounterKey(tenantId, ct, version, "misses"));
            var evictions = await db.StringGetAsync(CounterKey(tenantId, ct, version, "evictions"));
            var entries = await db.StringGetAsync(CounterKey(tenantId, ct, version, "entries"));

            totalHits += (long?)hits ?? 0;
            totalMisses += (long?)misses ?? 0;
            totalEvictions += (long?)evictions ?? 0;
            totalEntries += (long?)entries ?? 0;
        }

        var total = totalHits + totalMisses;
        var hitRate = total > 0 ? (double)totalHits / total * 100 : 0;

        return new SemanticCacheStats(totalEntries, totalHits, totalMisses, hitRate, totalEvictions, 0);
    }

    public async Task<IReadOnlyList<CacheKeyInfo>> ListKeysAsync(
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var indexKey = IndexKey(tenantId, cacheType, version);

        var skip = (page - 1) * pageSize;
        var members = await db.SortedSetRangeByRankAsync(indexKey, skip, skip + pageSize - 1);

        var results = new List<CacheKeyInfo>();

        foreach (var member in members)
        {
            var keyHashStr = member.ToString();
            if (string.IsNullOrEmpty(keyHashStr)) continue;

            var entryKey = EntryKey(tenantId, cacheType, version, keyHashStr);
            var entry = await db.HashGetAllAsync(entryKey);

            if (entry.Length == 0)
                continue;

            var dict = entry.ToDictionary(r => r.Name.ToString(), r => r.Value.ToString());

            if (!dict.TryGetValue("expires_at", out var expiresStr)
                || !long.TryParse(expiresStr, out var expiresMs))
                continue;

            if (DateTimeOffset.UtcNow >= DateTimeOffset.FromUnixTimeMilliseconds(expiresMs))
                continue;

            dict.TryGetValue("created_at", out var createdStr);
            dict.TryGetValue("hit_count", out var hitCountStr);

            results.Add(new CacheKeyInfo(
                keyHashStr,
                long.TryParse(createdStr, out var ct) ? DateTimeOffset.FromUnixTimeMilliseconds(ct) : DateTimeOffset.UtcNow,
                DateTimeOffset.FromUnixTimeMilliseconds(expiresMs),
                long.TryParse(hitCountStr, out var hc) ? hc : 0,
                Array.Empty<string>()));
        }

        return results;
    }

    public async Task<long> CountKeysAsync(
        TenantId tenantId,
        SemanticCacheType cacheType,
        SemanticCacheVersion version,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var indexKey = IndexKey(tenantId, cacheType, version);
        return await db.SortedSetLengthAsync(indexKey);
    }

    private string EntryKey(TenantId tenantId, SemanticCacheType cacheType, SemanticCacheVersion version, string keyHash) =>
        $"{_options.KeyPrefix}:{tenantId.Value:N}:{cacheType}:{version}:{keyHash}";

    private string IndexKey(TenantId tenantId, SemanticCacheType cacheType, SemanticCacheVersion version) =>
        $"{_options.KeyPrefix}:{tenantId.Value:N}:{cacheType}:{version}:index";

    private static string TagKey(TenantId tenantId, SemanticCacheType cacheType, SemanticCacheVersion version, string tag) =>
        $"{tenantId.Value:N}:{cacheType}:{version}:tag:{tag}";

    private static string CounterKey(TenantId tenantId, SemanticCacheType cacheType, SemanticCacheVersion? version, string counter) =>
        $"stats:{tenantId.Value:N}:{cacheType}:{version?.Value ?? "all"}:{counter}";

    private static string ComputeKeyHash(TenantId tenantId, SemanticCacheType cacheType, SemanticCacheVersion version, string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{tenantId}:{cacheType}:{version}:{key}"));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string EncodeEmbedding(double[] embedding)
    {
        return string.Join(",", embedding.Select(v => v.ToString("R", CultureInfo.InvariantCulture)));
    }

    private double[] DecodeEmbedding(string encoded)
    {
        var parts = encoded.Split(',');
        var embedding = new double[_dimensions];
        for (var i = 0; i < Math.Min(parts.Length, _dimensions); i++)
        {
            if (double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                embedding[i] = val;
        }
        return embedding;
    }

    private static double CosineSimilarity(double[] a, double[] b)
    {
        var len = Math.Min(a.Length, b.Length);
        var dot = 0d;
        for (var i = 0; i < len; i++)
            dot += a[i] * b[i];
        return Math.Max(0, dot);
    }

    private static async Task IncrementCounterAsync(IDatabase db, TenantId tenantId, SemanticCacheType cacheType, SemanticCacheVersion version, string counter, long delta)
    {
        var key = CounterKey(tenantId, cacheType, version, counter);
        await db.StringIncrementAsync(key);
    }

    private static async Task EvictExpiredEntryAsync(IDatabase db, string entryKey, string indexKey, string keyHash)
    {
        try
        {
            await db.KeyDeleteAsync(entryKey);
            await db.SortedSetRemoveAsync(indexKey, keyHash);
        }
        catch
        {
        }
    }
}

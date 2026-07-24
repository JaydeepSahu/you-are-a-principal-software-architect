using System.Collections.Concurrent;
using EnterpriseAiPlatform.Caching.Abstractions;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Caching.Engine;

public sealed class SemanticCacheEngine : ISemanticCache
{
    private sealed record CacheItem(
        string TenantId,
        string Prompt,
        float[] VectorEmbedding,
        CachedResponseEntry Entry,
        DateTimeOffset ExpiresAtUtc);

    private readonly ConcurrentDictionary<string, List<CacheItem>> _store = new();
    private readonly TimeSpan _defaultTtl = TimeSpan.FromHours(24);

    public Task<Result<SemanticCacheResult>> GetCachedResponseAsync(
        TenantId tenantId,
        string prompt,
        double minSimilarityThreshold = 0.95,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Task.FromResult(Result<SemanticCacheResult>.Success(new SemanticCacheResult(false, 0.0, null)));
        }

        string key = tenantId.Value.ToString();
        if (!_store.TryGetValue(key, out var items) || items.Count == 0)
        {
            return Task.FromResult(Result<SemanticCacheResult>.Success(new SemanticCacheResult(false, 0.0, null)));
        }

        var now = DateTimeOffset.UtcNow;
        var queryVector = GenerateVector(prompt);

        CacheItem? bestMatch = null;
        double maxSimilarity = 0.0;

        lock (items)
        {
            // Remove expired items
            items.RemoveAll(i => i.ExpiresAtUtc < now);

            foreach (var item in items)
            {
                double sim = ComputeCosineSimilarity(queryVector, item.VectorEmbedding);
                if (sim > maxSimilarity)
                {
                    maxSimilarity = sim;
                    bestMatch = item;
                }
            }
        }

        if (bestMatch != null && maxSimilarity >= minSimilarityThreshold)
        {
            var hitResult = new SemanticCacheResult(true, Math.Round(maxSimilarity, 4), bestMatch.Entry);
            return Task.FromResult(Result<SemanticCacheResult>.Success(hitResult));
        }

        return Task.FromResult(Result<SemanticCacheResult>.Success(new SemanticCacheResult(false, Math.Round(maxSimilarity, 4), null)));
    }

    public Task CacheResponseAsync(
        TenantId tenantId,
        string prompt,
        string responsePayload,
        string modelId,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        string key = tenantId.Value.ToString();
        var items = _store.GetOrAdd(key, _ => new List<CacheItem>());
        var vector = GenerateVector(prompt);
        var entry = new CachedResponseEntry(prompt, modelId, responsePayload, DateTimeOffset.UtcNow, 140);
        var expiresAt = DateTimeOffset.UtcNow.Add(ttl ?? _defaultTtl);

        lock (items)
        {
            items.Add(new CacheItem(key, prompt, vector, entry, expiresAt));
        }

        return Task.CompletedTask;
    }

    private static float[] GenerateVector(string text)
    {
        // Deterministic character n-gram term frequency vector normalization (128 dimensions)
        float[] vector = new float[128];
        string normalized = text.ToLowerInvariant().Trim();
        if (normalized.Length == 0) return vector;

        for (int i = 0; i < normalized.Length; i++)
        {
            int index = Math.Abs(normalized[i].GetHashCode()) % 128;
            vector[index] += 1.0f;
        }

        // L2 normalization
        double sumSq = 0.0;
        for (int i = 0; i < vector.Length; i++)
        {
            sumSq += vector[i] * vector[i];
        }

        float norm = (float)Math.Sqrt(sumSq);
        if (norm > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= norm;
            }
        }

        return vector;
    }

    private static double ComputeCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        double dotProduct = 0.0;
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
        }
        return Math.Max(0.0, Math.Min(1.0, dotProduct));
    }
}

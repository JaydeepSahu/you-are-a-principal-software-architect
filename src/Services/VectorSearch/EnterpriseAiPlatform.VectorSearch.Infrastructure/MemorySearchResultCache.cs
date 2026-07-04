using System.Collections.Concurrent;
using EnterpriseAiPlatform.VectorSearch.Application.Abstractions;

namespace EnterpriseAiPlatform.VectorSearch.Infrastructure;

public sealed class MemorySearchResultCache : ISearchResultCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();

    public int Count => _entries.Count(entry => entry.Value.ExpiresAtUtc > DateTimeOffset.UtcNow);

    public bool TryGet(string key, out CachedVectorSearchResult result)
    {
        result = default!;
        if (!_entries.TryGetValue(key, out var entry))
        {
            return false;
        }

        if (entry.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            _entries.TryRemove(key, out _);
            return false;
        }

        result = entry.Result;
        return true;
    }

    public void Store(string key, CachedVectorSearchResult result, TimeSpan ttl)
    {
        _entries[key] = new CacheEntry(result, DateTimeOffset.UtcNow.Add(ttl));
    }

    private sealed record CacheEntry(CachedVectorSearchResult Result, DateTimeOffset ExpiresAtUtc);
}

using StackExchange.Redis;

namespace EnterpriseAiPlatform.Infrastructure.Abstractions;

public sealed class RedisDistributedLeaseProvider(IConnectionMultiplexer redis) : IDistributedLeaseProvider
{
    private const string KeyPrefix = "eap:leases:";

    public async ValueTask<IDistributedLease?> TryAcquireAsync(
        string resource,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), "TTL must be greater than zero.");
        }

        var database = redis.GetDatabase();
        var key = $"{KeyPrefix}{resource.Trim().ToLowerInvariant()}";
        var owner = Guid.NewGuid().ToString("N");

        bool acquired = await database.StringSetAsync(key, owner, ttl, When.NotExists);
        if (!acquired)
        {
            return null;
        }

        return new RedisDistributedLease(database, key, owner, resource);
    }
}

internal sealed class RedisDistributedLease : IDistributedLease
{
    private const string ReleaseScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end";

    private readonly IDatabase _database;
    private readonly string _key;
    private int _isDisposed;

    public RedisDistributedLease(IDatabase database, string key, string owner, string resource)
    {
        _database = database;
        _key = key;
        Owner = owner;
        Resource = resource;
    }

    public string Resource { get; }

    public string Owner { get; }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
        {
            return;
        }

        try
        {
            await _database.ScriptEvaluateAsync(
                ReleaseScript,
                [new RedisKey(_key)],
                [new RedisValue(Owner)]);
        }
        catch
        {
            // Best effort release on shutdown
        }
    }
}

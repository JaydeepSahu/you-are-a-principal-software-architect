namespace EnterpriseAiPlatform.Infrastructure.Abstractions;

public interface IDistributedLease : IAsyncDisposable
{
    string Resource { get; }

    string Owner { get; }
}

public interface IDistributedLeaseProvider
{
    ValueTask<IDistributedLease?> TryAcquireAsync(
        string resource,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);
}

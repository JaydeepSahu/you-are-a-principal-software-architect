using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Contracts.Requests;
using EnterpriseAiPlatform.SemanticCache.Contracts.Responses;
using EnterpriseAiPlatform.SemanticCache.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.SemanticCache.Application.Cache;

public sealed record SetCacheCommand(SetCacheRequest Request) : ICommand<SetCacheResponse>;

public sealed class SetCacheHandler(
    ISemanticCacheStore store,
    ISemanticCacheMetrics metrics,
    IEmbeddingGenerator embeddingGenerator,
    IRequestContextAccessor requestContext)
    : ICommandHandler<SetCacheCommand, SetCacheResponse>
{
    public async Task<Result<SetCacheResponse>> Handle(SetCacheCommand command, CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var req = command.Request;

        if (!Enum.TryParse<SemanticCacheType>(req.CacheType, true, out var cacheType))
            return Result.Failure<SetCacheResponse>(SemanticCacheErrors.InvalidCacheType(req.CacheType));

        var version = new SemanticCacheVersion(req.Version);
        var embedding = req.Embedding ?? embeddingGenerator.Generate(req.Key);

        var ttl = req.TtlSeconds ?? DefaultTtl(cacheType);
        var tags = req.Tags?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        var keyHash = ComputeKeyHash(tenantId, cacheType, version, req.Key);

        await store.SetAsync(tenantId, cacheType, version, req.Key, embedding, req.Value, ttl, tags, cancellationToken);
        metrics.RecordCacheSet(tenantId, cacheType, version.Value);

        return Result.Success(new SetCacheResponse(keyHash, DateTimeOffset.UtcNow.Add(ttl)));
    }

    private static TimeSpan DefaultTtl(SemanticCacheType cacheType) => cacheType switch
    {
        SemanticCacheType.Response => TimeSpan.FromHours(1),
        SemanticCacheType.Prompt => TimeSpan.FromHours(24),
        SemanticCacheType.Conversation => TimeSpan.FromMinutes(30),
        _ => TimeSpan.FromHours(1)
    };

    private static string ComputeKeyHash(TenantId tenantId, SemanticCacheType cacheType, SemanticCacheVersion version, string key)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes($"{tenantId}:{cacheType}:{version}:{key}");
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public interface IEmbeddingGenerator
{
    double[] Generate(string text);
}

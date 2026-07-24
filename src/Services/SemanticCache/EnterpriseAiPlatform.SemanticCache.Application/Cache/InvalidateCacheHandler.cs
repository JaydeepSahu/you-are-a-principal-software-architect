using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Contracts.Requests;
using EnterpriseAiPlatform.SemanticCache.Contracts.Responses;
using EnterpriseAiPlatform.SemanticCache.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.SemanticCache.Application.Cache;

public sealed record InvalidateCacheCommand(InvalidateCacheRequest Request) : ICommand<InvalidateCacheResponse>;

public sealed class InvalidateCacheHandler(
    ISemanticCacheStore store,
    ISemanticCacheMetrics metrics,
    IRequestContextAccessor requestContext)
    : ICommandHandler<InvalidateCacheCommand, InvalidateCacheResponse>
{
    public async Task<Result<InvalidateCacheResponse>> Handle(InvalidateCacheCommand command, CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var req = command.Request;

        if (!Enum.TryParse<SemanticCacheType>(req.CacheType, true, out var cacheType))
            return Result.Failure<InvalidateCacheResponse>(SemanticCacheErrors.InvalidCacheType(req.CacheType));

        SemanticCacheVersion? version = req.Version is null ? null : new SemanticCacheVersion(req.Version);

        var count = await store.InvalidateAsync(tenantId, cacheType, version, req.Key, req.Tag, cancellationToken);
        metrics.RecordCacheInvalidation(tenantId, cacheType, version?.Value ?? "all", count);

        return Result.Success(new InvalidateCacheResponse(count, 0));
    }
}

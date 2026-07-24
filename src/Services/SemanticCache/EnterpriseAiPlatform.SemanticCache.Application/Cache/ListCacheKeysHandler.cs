using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Contracts.Requests;
using EnterpriseAiPlatform.SemanticCache.Contracts.Responses;
using EnterpriseAiPlatform.SemanticCache.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using ContractCacheKeyInfo = EnterpriseAiPlatform.SemanticCache.Contracts.Responses.CacheKeyInfo;

namespace EnterpriseAiPlatform.SemanticCache.Application.Cache;

public sealed record ListCacheKeysQuery(ListCacheKeysRequest Request) : IQuery<ListCacheKeysResponse>;

public sealed class ListCacheKeysHandler(
    ISemanticCacheStore store,
    IRequestContextAccessor requestContext)
    : IQueryHandler<ListCacheKeysQuery, ListCacheKeysResponse>
{
    public async Task<Result<ListCacheKeysResponse>> Handle(ListCacheKeysQuery query, CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var req = query.Request;

        if (!Enum.TryParse<SemanticCacheType>(req.CacheType, true, out var cacheType))
            return Result.Failure<ListCacheKeysResponse>(SemanticCacheErrors.InvalidCacheType(req.CacheType));

        var version = new SemanticCacheVersion(req.Version);
        var pageSize = Math.Clamp(req.PageSize, 1, 200);
        var page = Math.Max(req.Page, 1);

        var keys = await store.ListKeysAsync(tenantId, cacheType, version, page, pageSize, cancellationToken);
        var total = await store.CountKeysAsync(tenantId, cacheType, version, cancellationToken);

        var keyInfos = keys.Select(k => new ContractCacheKeyInfo(k.KeyHash, k.CreatedAtUtc, k.ExpiresAtUtc, k.HitCount, k.Tags)).ToList();

        return Result.Success(new ListCacheKeysResponse(keyInfos, (int)total, page, pageSize));
    }
}

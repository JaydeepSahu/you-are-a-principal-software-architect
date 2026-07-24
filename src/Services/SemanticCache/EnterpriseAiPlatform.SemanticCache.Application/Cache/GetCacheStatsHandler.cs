using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Contracts.Requests;
using EnterpriseAiPlatform.SemanticCache.Contracts.Responses;
using EnterpriseAiPlatform.SemanticCache.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.SemanticCache.Application.Cache;

public sealed record GetCacheStatsQuery(GetCacheStatsRequest Request) : IQuery<GetCacheStatsResponse>;

public sealed class GetCacheStatsHandler(
    ISemanticCacheStore store,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetCacheStatsQuery, GetCacheStatsResponse>
{
    public async Task<Result<GetCacheStatsResponse>> Handle(GetCacheStatsQuery query, CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var req = query.Request;

        SemanticCacheType? cacheType = null;
        if (req.CacheType is not null && Enum.TryParse<SemanticCacheType>(req.CacheType, true, out var ct))
            cacheType = ct;

        SemanticCacheVersion? version = null;
        if (req.Version is not null)
            version = new SemanticCacheVersion(req.Version);

        var stats = await store.GetStatsAsync(tenantId, cacheType, version, cancellationToken);

        return Result.Success(new GetCacheStatsResponse(
            stats.TotalEntries,
            stats.TotalHits,
            stats.TotalMisses,
            stats.HitRatePercent,
            stats.TotalEvictions,
            0));
    }
}

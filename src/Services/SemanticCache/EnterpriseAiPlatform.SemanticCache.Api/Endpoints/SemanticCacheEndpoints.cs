using EnterpriseAiPlatform.SemanticCache.Application.Cache;
using EnterpriseAiPlatform.SemanticCache.Contracts.Requests;
using MediatR;

namespace EnterpriseAiPlatform.SemanticCache.Api.Endpoints;

public static class SemanticCacheEndpoints
{
    public static IEndpointRouteBuilder MapSemanticCacheEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/semantic-cache")
            .WithTags("SemanticCache")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("cache", async (SetCacheRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SetCacheCommand(request), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .Produces(200)
        .Produces(400)
        .WithName("SetCache")
        .WithSummary("Store a value in the semantic cache");

        group.MapGet("cache", async (
            string cacheType,
            string version,
            string key,
            double[]? queryEmbedding,
            double? minSimilarity,
            ISender sender,
            CancellationToken ct) =>
        {
            var request = new GetCacheRequest(cacheType, version, key, queryEmbedding, minSimilarity);
            var result = await sender.Send(new GetCacheQuery(request), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .Produces(200)
        .Produces(400)
        .WithName("GetCache")
        .WithSummary("Retrieve a cached value by key or semantic similarity");

        group.MapDelete("cache", async (InvalidateCacheRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new InvalidateCacheCommand(request), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .Produces(200)
        .Produces(400)
        .WithName("InvalidateCache")
        .WithSummary("Invalidate cache entries by key, tag, or version");

        group.MapGet("stats", async (string? cacheType, string? version, ISender sender, CancellationToken ct) =>
        {
            var request = new GetCacheStatsRequest(cacheType, version);
            var result = await sender.Send(new GetCacheStatsQuery(request), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .Produces(200)
        .Produces(400)
        .WithName("GetCacheStats")
        .WithSummary("Get cache hit/miss/eviction statistics");

        group.MapGet("keys", async (
            string cacheType,
            string version,
            int page,
            int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var request = new ListCacheKeysRequest(cacheType, version, page, pageSize);
            var result = await sender.Send(new ListCacheKeysQuery(request), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .Produces(200)
        .Produces(400)
        .WithName("ListCacheKeys")
        .WithSummary("List cache keys with pagination");

        return app;
    }
}

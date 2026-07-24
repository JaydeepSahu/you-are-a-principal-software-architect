using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Application.Abstractions;
using EnterpriseAiPlatform.SemanticCache.Application.Cache;
using EnterpriseAiPlatform.SemanticCache.Application.Validation;

using EnterpriseAiPlatform.SemanticCache.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EnterpriseAiPlatform.SemanticCache.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSemanticCacheInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SemanticCacheOptions>(configuration.GetSection("SemanticCache"));
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<ISemanticCacheMetrics, SemanticCacheMetrics>();
        services.AddSingleton<IEmbeddingGenerator, HashingEmbeddingGenerator>();

        // Register Redis if connection string is configured
        var redisConnString = configuration["SemanticCache:RedisConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConnString))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnString));
            services.AddScoped<ISemanticCacheStore, RedisSemanticCacheStore>();
        }
        else
        {
            services.AddSingleton<ISemanticCacheStore, InMemorySemanticCacheStore>();
        }

        return services;
    }
}

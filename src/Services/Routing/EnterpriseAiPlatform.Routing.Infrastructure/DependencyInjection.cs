using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Infrastructure.Abstractions;
using EnterpriseAiPlatform.Routing.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Infrastructure.Persistence;
using EnterpriseAiPlatform.Routing.Infrastructure.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EnterpriseAiPlatform.Routing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRoutingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextAccessor>().Current);

        var redisCs = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisCs))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisCs));
            services.AddSingleton<IRoutingConfigurationRepository, RedisRoutingConfigurationRepository>();
            services.AddSingleton<IDistributedLeaseProvider, RedisDistributedLeaseProvider>();
        }
        else
        {
            services.AddSingleton<IRoutingConfigurationRepository, InMemoryRoutingConfigurationRepository>();
        }

        // MemoryRoutingTelemetry is a bounded in-process ring-buffer (max 500 snapshots).
        // Routing decisions are exported via OpenTelemetry metrics. This ring-buffer
        // exists only for same-process diagnostic queries.
        services.AddSingleton<IRoutingTelemetry, MemoryRoutingTelemetry>();

        return services;
    }

    public static IServiceCollection AddRoutingInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextAccessor>().Current);
        services.AddSingleton<IRoutingConfigurationRepository, InMemoryRoutingConfigurationRepository>();
        services.AddSingleton<IRoutingTelemetry, MemoryRoutingTelemetry>();
        return services;
    }
}


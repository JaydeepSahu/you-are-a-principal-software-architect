using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EnterpriseAiPlatform.Infrastructure.Abstractions.EventBus;

public static class EventBusDependencyInjection
{
    public static IServiceCollection AddEnterpriseEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var redisCs = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisCs))
        {
            if (!services.Any(s => s.ServiceType == typeof(IConnectionMultiplexer)))
            {
                services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisCs));
            }

            services.AddSingleton<IEventBus, RedisEventBus>();
        }
        else
        {
            services.AddSingleton<IEventBus, ChannelEventBus>();
        }

        return services;
    }
}

using EnterpriseAiPlatform.AiGateway.Application.RateLimiting;
using EnterpriseAiPlatform.AiGateway.Application.Telemetry;
using EnterpriseAiPlatform.AiGateway.Infrastructure.RateLimiting;
using EnterpriseAiPlatform.AiGateway.Infrastructure.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EnterpriseAiPlatform.AiGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAiGatewayInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<GatewayRateLimitOptions>()
            .Bind(configuration.GetSection(GatewayRateLimitOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.RedisConnectionString), "Gateway rate-limiting Redis connection string is required.")
            .Validate(options => options.PermitLimit > 0, "Gateway rate-limit permit limit must be positive.")
            .Validate(options => options.WindowSeconds is >= 1 and <= 3600, "Gateway rate-limit window must be between 1 and 3600 seconds.")
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            Microsoft.Extensions.Options.IOptions<GatewayRateLimitOptions> options =
                serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<GatewayRateLimitOptions>>();

            return ConnectionMultiplexer.Connect(options.Value.RedisConnectionString);
        });

        services.AddScoped<IGatewayRateLimiter, RedisGatewayRateLimiter>();
        services.AddSingleton<IGatewayMetrics, SystemDiagnosticsGatewayMetrics>();

        return services;
    }
}

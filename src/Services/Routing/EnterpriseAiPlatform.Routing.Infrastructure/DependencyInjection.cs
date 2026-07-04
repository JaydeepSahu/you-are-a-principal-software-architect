using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Infrastructure.Telemetry;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Routing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRoutingInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<IRoutingConfigurationRepository, InMemoryRoutingConfigurationRepository>();
        services.AddSingleton<IRoutingTelemetry, MemoryRoutingTelemetry>();

        return services;
    }
}

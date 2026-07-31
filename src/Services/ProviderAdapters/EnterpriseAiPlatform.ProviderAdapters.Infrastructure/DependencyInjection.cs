using EnterpriseAiPlatform.ProviderAdapters.Application;
using EnterpriseAiPlatform.ProviderAdapters.Application.Abstractions;
using EnterpriseAiPlatform.ProviderAdapters.Domain;
using EnterpriseAiPlatform.ProviderAdapters.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.ProviderAdapters.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProviderAdapters(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(ProviderResiliencePolicy.Default);
        services.AddTransient<ResilientHttpClientHandler>();
        services.AddHttpClient("ProviderHttpClient")
            .AddHttpMessageHandler<ResilientHttpClientHandler>();

        services.AddSingleton<IProviderPluginRegistry, InMemoryProviderPluginRegistry>();
        services.AddSingleton<IProviderCircuitBreakerStore, InMemoryProviderCircuitBreakerStore>();
        services.AddSingleton<IProviderTelemetry, InMemoryProviderTelemetry>();
        services.AddSingleton<IProviderOrchestrator, ProviderOrchestrator>();

        return services;
    }
}

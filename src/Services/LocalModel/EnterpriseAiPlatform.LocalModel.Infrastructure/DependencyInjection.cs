using EnterpriseAiPlatform.LocalModel.Application;
using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.LocalModel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLocalModel(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpClient();
        var providers = configuration.GetSection("LocalModel:Providers").Get<List<LocalModelProviderResponse>>() ?? [];
        services.AddSingleton<IReadOnlyCollection<LocalModelProviderResponse>>(providers);
        services.AddSingleton<ILocalModelPluginRegistry, InMemoryLocalModelPluginRegistry>();
        services.AddSingleton<ILocalModelHealthStore, InMemoryLocalModelHealthStore>();
        services.AddSingleton<ILocalModelLoadBalancer, LocalModelLoadBalancer>();
        services.AddSingleton<ILocalModelOrchestrator, LocalModelOrchestrator>();

        return services;
    }
}

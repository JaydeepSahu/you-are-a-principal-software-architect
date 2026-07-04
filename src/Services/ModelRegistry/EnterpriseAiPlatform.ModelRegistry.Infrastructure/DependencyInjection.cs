using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.ModelRegistry.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddModelRegistryInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<IModelRegistryRepository, InMemoryModelRegistryRepository>();

        return services;
    }
}

using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Infrastructure.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.ModelRegistry.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddModelRegistryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();

        string? connectionString = configuration.GetConnectionString("PostgreSQL")
                                   ?? configuration.GetConnectionString("ModelRegistryDatabase");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<ModelRegistryDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "model_registry")));

            services.AddScoped<IModelRegistryRepository, EfCoreModelRegistryRepository>();
        }
        else
        {
            services.AddSingleton<IModelRegistryRepository, InMemoryModelRegistryRepository>();
        }

        return services;
    }

    public static IServiceCollection AddModelRegistryInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<IModelRegistryRepository, InMemoryModelRegistryRepository>();
        return services;
    }
}

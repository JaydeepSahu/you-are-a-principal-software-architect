using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPromptIntelligenceInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IProfileRepository, InMemoryProfileRepository>();
        services.AddScoped<ISessionRepository, InMemorySessionRepository>();
        services.AddSingleton<IPromptOptimizer, PromptOptimizer>();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();

        return services;
    }
}

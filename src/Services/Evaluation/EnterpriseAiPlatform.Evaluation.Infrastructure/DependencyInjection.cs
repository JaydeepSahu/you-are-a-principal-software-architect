using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Evaluation.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Evaluation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEvaluationInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<IEvaluationStore, InMemoryEvaluationStore>();

        return services;
    }
}

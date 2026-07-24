using EnterpriseAiPlatform.Agents.Application.Commands;
using EnterpriseAiPlatform.Agents.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Agents.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentServices(this IServiceCollection services)
    {
        services.AddAgentInfrastructure();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ExecuteAgentPlanCommand).Assembly));
        return services;
    }
}

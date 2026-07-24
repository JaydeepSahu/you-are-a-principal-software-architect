using EnterpriseAiPlatform.Agents.Api;
using EnterpriseAiPlatform.Agents.Sdk.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Agents.Sdk;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentSdk(this IServiceCollection services)
    {
        services.AddAgentServices();
        return services;
    }
}

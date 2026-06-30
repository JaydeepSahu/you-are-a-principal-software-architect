using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.AiGateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAiGatewayApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(TimeProvider.System);
        return services;
    }
}

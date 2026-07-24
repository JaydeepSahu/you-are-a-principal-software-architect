using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.BackgroundWorkers.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundWorkersApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}

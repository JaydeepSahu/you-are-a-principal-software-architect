using EnterpriseAiPlatform.Routing.Application.Selection;
using EnterpriseAiPlatform.Routing.Application.Validation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Routing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRoutingApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddSingleton<IRoutingEvaluator, RoutingEvaluator>();

        return services;
    }
}

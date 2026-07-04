using EnterpriseAiPlatform.Knowledge.Application.Validation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Knowledge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddKnowledgeApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

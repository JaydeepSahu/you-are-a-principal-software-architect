using EnterpriseAiPlatform.PromptIntelligence.Application.Validation;
using EnterpriseAiPlatform.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.PromptIntelligence.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPromptIntelligenceApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(TimeProvider.System);
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

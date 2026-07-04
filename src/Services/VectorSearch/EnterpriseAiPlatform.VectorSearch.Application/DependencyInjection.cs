using EnterpriseAiPlatform.VectorSearch.Application.Validation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.VectorSearch.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVectorSearchApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

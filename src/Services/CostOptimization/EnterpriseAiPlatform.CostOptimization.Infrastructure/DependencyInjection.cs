using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.CostOptimization.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCostOptimizationInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IBudgetRepository, InMemoryBudgetRepository>();
        services.AddSingleton<IDepartmentQuotaRepository, InMemoryDepartmentQuotaRepository>();
        services.AddSingleton<IUserQuotaRepository, InMemoryUserQuotaRepository>();
        services.AddSingleton<IModelCostRepository, InMemoryModelCostRepository>();
        services.AddSingleton<ICostRecordRepository, InMemoryCostRecordRepository>();
        services.AddSingleton<IAlertRepository, InMemoryAlertRepository>();
        services.AddSingleton<IRoutingRuleRepository, InMemoryRoutingRuleRepository>();

        return services;
    }
}

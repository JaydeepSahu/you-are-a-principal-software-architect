using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.CostOptimization.Infrastructure.Seeding;

public static class CostOptimizationSeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var budgetRepo = scope.ServiceProvider.GetRequiredService<IBudgetRepository>();
        var deptQuotaRepo = scope.ServiceProvider.GetRequiredService<IDepartmentQuotaRepository>();
        var modelCostRepo = scope.ServiceProvider.GetRequiredService<IModelCostRepository>();
        
        var tenantId = TenantId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        
        var existingBudgets = await budgetRepo.GetAllAsync(tenantId, 0, 1);
        if (existingBudgets.Count > 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        // Model Costs
        var models = new[]
        {
            ModelCost.Create(tenantId, "AzureOpenAI", "gpt-4o", "GPT-4o", 0.005m, 0.015m, 0m, 0m, now.AddDays(-30)),
            ModelCost.Create(tenantId, "Anthropic", "claude-3-5-sonnet", "Claude 3.5 Sonnet", 0.003m, 0.015m, 0m, 0m, now.AddDays(-30)),
            ModelCost.Create(tenantId, "GoogleGemini", "gemini-1-5-pro", "Gemini 1.5 Pro", 0.0035m, 0.0105m, 0m, 0m, now.AddDays(-30)),
            ModelCost.Create(tenantId, "SelfHostedVllm", "deepseek-coder", "DeepSeek Coder", 0.000m, 0.000m, 0m, 0.05m, now.AddDays(-30))
        };

        foreach (var m in models)
        {
            await modelCostRepo.AddAsync(m);
        }

        // Budgets
        var itBudget = Budget.Create(tenantId, "IT Department AI Budget", "Annual budget for IT", CostAmount.Create(50000m), BudgetPeriod.Monthly, now.Year, now.Month, null, "dept-it", null);
        var engBudget = Budget.Create(tenantId, "Engineering AI Budget", "Monthly budget for Engineering", CostAmount.Create(100000m), BudgetPeriod.Monthly, now.Year, now.Month, null, "dept-eng", null);
        
        await budgetRepo.AddAsync(itBudget);
        await budgetRepo.AddAsync(engBudget);

        // Department Quotas
        var itQuota = DepartmentQuota.Create(tenantId, "dept-it", "IT", CostAmount.Create(50000m), 100, null);
        var engQuota = DepartmentQuota.Create(tenantId, "dept-eng", "Engineering", CostAmount.Create(100000m), 250, null);

        await deptQuotaRepo.AddAsync(itQuota);
        await deptQuotaRepo.AddAsync(engQuota);
    }
}

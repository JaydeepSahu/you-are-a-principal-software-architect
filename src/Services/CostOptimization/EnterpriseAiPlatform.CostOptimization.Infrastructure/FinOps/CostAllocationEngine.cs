using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Domain.Entities;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Infrastructure.FinOps;

public sealed class CostAllocationEngine : ICostAllocationEngine
{
    private static readonly List<DepartmentChargeback> DefaultBreakdown = new()
    {
        new DepartmentChargeback("dept-eng", "Software Engineering", "CC-101", 450_000_000, 22_500.00m, 65.6),
        new DepartmentChargeback("dept-data", "Data & AI Science", "CC-102", 150_000_000, 7_500.00m, 21.9),
        new DepartmentChargeback("dept-product", "Product Management", "CC-103", 50_000_000, 2_500.00m, 7.3),
        new DepartmentChargeback("dept-ops", "DevOps & SRE", "CC-104", 35_000_000, 1_750.00m, 5.2)
    };

    public Task<Result<IReadOnlyList<DepartmentChargeback>>> GenerateChargebackReportAsync(
        TenantId tenantId,
        DateTimeOffset? month = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DepartmentChargeback> list = DefaultBreakdown;
        return Task.FromResult(Result<IReadOnlyList<DepartmentChargeback>>.Success(list));
    }

    public Task<Result<CostForecastResult>> ForecastMonthlySpendAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        int currentDay = Math.Max(1, now.Day);

        decimal currentSpend = 34_250.00m;
        decimal dailyRunRate = currentSpend / currentDay;
        decimal projectedSpend = Math.Round(dailyRunRate * daysInMonth, 2);
        decimal budgetLimit = 50_000.00m;

        double projectedPercentage = (double)(projectedSpend / budgetLimit) * 100.0;
        string status = projectedPercentage > 100.0 ? "Exceeded" : (projectedPercentage >= 85.0 ? "Warning" : "OnTrack");

        var forecast = new CostForecastResult(
            tenantId.Value.ToString(),
            currentSpend,
            Math.Round(dailyRunRate, 2),
            projectedSpend,
            budgetLimit,
            Math.Round(projectedPercentage, 1),
            status
        );

        return Task.FromResult(Result<CostForecastResult>.Success(forecast));
    }

    public Task<Result<TenantBudgetStatus>> GetTenantBudgetStatusAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        decimal budget = 50_000.00m;
        decimal consumed = 34_250.00m;
        double usagePct = (double)(consumed / budget) * 100.0;

        var status = new TenantBudgetStatus(
            tenantId.Value.ToString(),
            budget,
            consumed,
            Math.Round(usagePct, 1),
            SoftWarningTriggered: usagePct >= 75.0,
            HardQuotaBlockTriggered: usagePct >= 100.0,
            DefaultBreakdown
        );

        return Task.FromResult(Result<TenantBudgetStatus>.Success(status));
    }
}

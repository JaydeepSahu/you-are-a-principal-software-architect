using EnterpriseAiPlatform.CostOptimization.Domain.Entities;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Application.Abstractions;

public interface ICostAllocationEngine
{
    Task<Result<IReadOnlyList<DepartmentChargeback>>> GenerateChargebackReportAsync(
        TenantId tenantId,
        DateTimeOffset? month = null,
        CancellationToken cancellationToken = default);

    Task<Result<CostForecastResult>> ForecastMonthlySpendAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    Task<Result<TenantBudgetStatus>> GetTenantBudgetStatusAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}

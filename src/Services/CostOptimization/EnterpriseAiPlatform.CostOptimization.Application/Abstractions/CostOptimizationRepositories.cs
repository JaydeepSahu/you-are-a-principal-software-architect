using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Application.Abstractions;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(BudgetId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Budget>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Budget>> QueryAsync(TenantId tenantId, string? departmentId, string? projectId, BudgetStatus? status, int? year, int? month, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(TenantId tenantId, string? departmentId, string? projectId, BudgetStatus? status, int? year, int? month, CancellationToken cancellationToken = default);
    Task AddAsync(Budget budget, CancellationToken cancellationToken = default);
    Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default);
}

public interface IDepartmentQuotaRepository
{
    Task<DepartmentQuota?> GetByIdAsync(DepartmentQuotaId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<DepartmentQuota?> GetByDepartmentIdAsync(string departmentId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepartmentQuota>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepartmentQuota>> QueryAsync(TenantId tenantId, string? departmentId, string? userId, bool? isActive, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(TenantId tenantId, string? departmentId, string? userId, bool? isActive, CancellationToken cancellationToken = default);
    Task AddAsync(DepartmentQuota quota, CancellationToken cancellationToken = default);
    Task UpdateAsync(DepartmentQuota quota, CancellationToken cancellationToken = default);
}

public interface IUserQuotaRepository
{
    Task<UserQuota?> GetByIdAsync(UserQuotaId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<UserQuota?> GetByUserIdAsync(string userId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserQuota>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserQuota>> QueryAsync(TenantId tenantId, string? departmentId, string? userId, bool? isActive, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(TenantId tenantId, string? departmentId, string? userId, bool? isActive, CancellationToken cancellationToken = default);
    Task AddAsync(UserQuota quota, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserQuota quota, CancellationToken cancellationToken = default);
}

public interface IModelCostRepository
{
    Task<ModelCost?> GetByIdAsync(ModelCostId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<ModelCost?> GetByProviderModelAsync(string providerId, string modelId, TenantId tenantId, DateTimeOffset? effectiveAt = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModelCost>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModelCost>> QueryAsync(TenantId tenantId, string? providerId, string? modelId, bool? isActive, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(TenantId tenantId, string? providerId, string? modelId, bool? isActive, CancellationToken cancellationToken = default);
    Task AddAsync(ModelCost modelCost, CancellationToken cancellationToken = default);
    Task UpdateAsync(ModelCost modelCost, CancellationToken cancellationToken = default);
}

public interface ICostRecordRepository
{
    Task<CostRecord?> GetByIdAsync(CostRecordId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CostRecord>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CostRecord>> QueryAsync(TenantId tenantId, string? userId, string? departmentId, string? providerId, string? modelId, CostCategory? category, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(TenantId tenantId, string? userId, string? departmentId, string? providerId, string? modelId, CostCategory? category, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default);
    Task AddAsync(CostRecord record, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<CostRecord> records, CancellationToken cancellationToken = default);
    Task<CostAggregation> GetAggregationAsync(TenantId tenantId, int year, int month, string? departmentId, string? userId, CancellationToken cancellationToken = default);
}

public interface IAlertRepository
{
    Task<Alert?> GetByIdAsync(AlertId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Alert>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Alert>> QueryAsync(TenantId tenantId, string? departmentId, string? userId, AlertStatus? status, AlertSeverity? severity, string? type, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(TenantId tenantId, string? departmentId, string? userId, AlertStatus? status, AlertSeverity? severity, string? type, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Alert>> GetActiveAlertsAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Alert alert, CancellationToken cancellationToken = default);
    Task UpdateAsync(Alert alert, CancellationToken cancellationToken = default);
}

public interface IRoutingRuleRepository
{
    Task<RoutingRule?> GetByIdAsync(RoutingRuleId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoutingRule>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoutingRule>> QueryAsync(TenantId tenantId, string? sourceModelId, string? targetModelId, string? triggerType, bool? isEnabled, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(TenantId tenantId, string? sourceModelId, string? targetModelId, string? triggerType, bool? isEnabled, CancellationToken cancellationToken = default);
    Task AddAsync(RoutingRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(RoutingRule rule, CancellationToken cancellationToken = default);
}

public sealed record CostAggregation(
    decimal TotalCost,
    int TotalRequests,
    int TotalInputTokens,
    int TotalOutputTokens,
    Dictionary<string, int> RequestsByProvider,
    Dictionary<string, int> RequestsByModel,
    Dictionary<string, int> RequestsByCategory,
    Dictionary<string, decimal> CostByProvider,
    Dictionary<string, decimal> CostByModel,
    Dictionary<string, decimal> CostByCategory,
    List<DailyAggregation> DailyBreakdown);

public sealed record DailyAggregation(int Day, decimal Cost, int Requests, int Tokens);

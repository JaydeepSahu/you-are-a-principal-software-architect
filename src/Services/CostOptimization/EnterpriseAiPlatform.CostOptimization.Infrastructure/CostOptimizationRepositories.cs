using System.Collections.Concurrent;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Infrastructure;

public sealed class InMemoryBudgetRepository : IBudgetRepository
{
    private readonly ConcurrentDictionary<Guid, Budget> _budgets = new();

    public Task<Budget?> GetByIdAsync(BudgetId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        _budgets.TryGetValue(id.Value, out var budget);
        return Task.FromResult(budget?.TenantId == tenantId ? budget : null);
    }

    public Task<IReadOnlyList<Budget>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var results = _budgets.Values
            .Where(b => b.TenantId == tenantId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();
        return Task.FromResult<IReadOnlyList<Budget>>(results);
    }

    public Task<IReadOnlyList<Budget>> QueryAsync(TenantId tenantId, string? departmentId, string? projectId, BudgetStatus? status, int? year, int? month, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _budgets.Values.Where(b => b.TenantId == tenantId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(b => b.DepartmentId == departmentId);

        if (!string.IsNullOrEmpty(projectId))
            query = query.Where(b => b.ProjectId == projectId);

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        if (year.HasValue)
            query = query.Where(b => b.PeriodYear == year.Value);

        if (month.HasValue)
            query = query.Where(b => b.PeriodMonth == month.Value);

        var results = query.OrderByDescending(b => b.CreatedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<Budget>>(results);
    }

    public Task<int> CountAsync(TenantId tenantId, string? departmentId, string? projectId, BudgetStatus? status, int? year, int? month, CancellationToken cancellationToken = default)
    {
        var query = _budgets.Values.Where(b => b.TenantId == tenantId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(b => b.DepartmentId == departmentId);

        if (!string.IsNullOrEmpty(projectId))
            query = query.Where(b => b.ProjectId == projectId);

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        if (year.HasValue)
            query = query.Where(b => b.PeriodYear == year.Value);

        if (month.HasValue)
            query = query.Where(b => b.PeriodMonth == month.Value);

        return Task.FromResult(query.Count());
    }

    public Task AddAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _budgets[budget.Id.Value] = budget;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _budgets[budget.Id.Value] = budget;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryDepartmentQuotaRepository : IDepartmentQuotaRepository
{
    private readonly ConcurrentDictionary<Guid, DepartmentQuota> _quotas = new();

    public Task<DepartmentQuota?> GetByIdAsync(DepartmentQuotaId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        _quotas.TryGetValue(id.Value, out var quota);
        return Task.FromResult(quota?.TenantId == tenantId ? quota : null);
    }

    public Task<DepartmentQuota?> GetByDepartmentIdAsync(string departmentId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var quota = _quotas.Values.FirstOrDefault(q => q.TenantId == tenantId && q.DepartmentId == departmentId);
        return Task.FromResult(quota);
    }

    public Task<IReadOnlyList<DepartmentQuota>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var results = _quotas.Values.Where(q => q.TenantId == tenantId).OrderByDescending(q => q.CreatedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<DepartmentQuota>>(results);
    }

    public Task<IReadOnlyList<DepartmentQuota>> QueryAsync(TenantId tenantId, string? departmentId, string? userId, bool? isActive, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _quotas.Values.Where(q => q.TenantId == tenantId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(q => q.DepartmentId == departmentId);

        if (isActive.HasValue)
            query = query.Where(q => q.IsActive == isActive.Value);

        var results = query.OrderByDescending(q => q.CreatedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<DepartmentQuota>>(results);
    }

    public Task<int> CountAsync(TenantId tenantId, string? departmentId, string? userId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _quotas.Values.Where(q => q.TenantId == tenantId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(q => q.DepartmentId == departmentId);

        if (isActive.HasValue)
            query = query.Where(q => q.IsActive == isActive.Value);

        return Task.FromResult(query.Count());
    }

    public Task AddAsync(DepartmentQuota quota, CancellationToken cancellationToken = default)
    {
        _quotas[quota.Id.Value] = quota;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DepartmentQuota quota, CancellationToken cancellationToken = default)
    {
        _quotas[quota.Id.Value] = quota;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryUserQuotaRepository : IUserQuotaRepository
{
    private readonly ConcurrentDictionary<Guid, UserQuota> _quotas = new();

    public Task<UserQuota?> GetByIdAsync(UserQuotaId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        _quotas.TryGetValue(id.Value, out var quota);
        return Task.FromResult(quota?.TenantId == tenantId ? quota : null);
    }

    public Task<UserQuota?> GetByUserIdAsync(string userId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var quota = _quotas.Values.FirstOrDefault(q => q.TenantId == tenantId && q.UserId == userId);
        return Task.FromResult(quota);
    }

    public Task<IReadOnlyList<UserQuota>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var results = _quotas.Values.Where(q => q.TenantId == tenantId).OrderByDescending(q => q.CreatedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<UserQuota>>(results);
    }

    public Task<IReadOnlyList<UserQuota>> QueryAsync(TenantId tenantId, string? departmentId, string? userId, bool? isActive, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _quotas.Values.Where(q => q.TenantId == tenantId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(q => q.DepartmentId == departmentId);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(q => q.UserId == userId);

        if (isActive.HasValue)
            query = query.Where(q => q.IsActive == isActive.Value);

        var results = query.OrderByDescending(q => q.CreatedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<UserQuota>>(results);
    }

    public Task<int> CountAsync(TenantId tenantId, string? departmentId, string? userId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _quotas.Values.Where(q => q.TenantId == tenantId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(q => q.DepartmentId == departmentId);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(q => q.UserId == userId);

        if (isActive.HasValue)
            query = query.Where(q => q.IsActive == isActive.Value);

        return Task.FromResult(query.Count());
    }

    public Task AddAsync(UserQuota quota, CancellationToken cancellationToken = default)
    {
        _quotas[quota.Id.Value] = quota;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserQuota quota, CancellationToken cancellationToken = default)
    {
        _quotas[quota.Id.Value] = quota;
        return Task.CompletedTask;
    }
}

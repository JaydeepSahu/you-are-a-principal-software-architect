using System.Collections.Concurrent;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Infrastructure;

public sealed class InMemoryModelCostRepository : IModelCostRepository
{
    private readonly ConcurrentDictionary<Guid, ModelCost> _modelCosts = new();

    public Task<ModelCost?> GetByIdAsync(ModelCostId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        _modelCosts.TryGetValue(id.Value, out var modelCost);
        return Task.FromResult(modelCost?.TenantId == tenantId ? modelCost : null);
    }

    public Task<ModelCost?> GetByProviderModelAsync(string providerId, string modelId, TenantId tenantId, DateTimeOffset? effectiveAt = null, CancellationToken cancellationToken = default)
    {
        var effective = effectiveAt ?? DateTimeOffset.UtcNow;
        var modelCost = _modelCosts.Values
            .Where(m => m.TenantId == tenantId && m.ProviderId == providerId && m.ModelId == modelId)
            .FirstOrDefault(m => m.IsEffectiveAt(effective));
        return Task.FromResult(modelCost);
    }

    public Task<IReadOnlyList<ModelCost>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var results = _modelCosts.Values.Where(m => m.TenantId == tenantId).OrderByDescending(m => m.CreatedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<ModelCost>>(results);
    }

    public Task<IReadOnlyList<ModelCost>> QueryAsync(TenantId tenantId, string? providerId, string? modelId, bool? isActive, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _modelCosts.Values.Where(m => m.TenantId == tenantId);

        if (!string.IsNullOrEmpty(providerId))
            query = query.Where(m => m.ProviderId == providerId);

        if (!string.IsNullOrEmpty(modelId))
            query = query.Where(m => m.ModelId == modelId);

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        var results = query.OrderByDescending(m => m.CreatedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<ModelCost>>(results);
    }

    public Task<int> CountAsync(TenantId tenantId, string? providerId, string? modelId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _modelCosts.Values.Where(m => m.TenantId == tenantId);

        if (!string.IsNullOrEmpty(providerId))
            query = query.Where(m => m.ProviderId == providerId);

        if (!string.IsNullOrEmpty(modelId))
            query = query.Where(m => m.ModelId == modelId);

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        return Task.FromResult(query.Count());
    }

    public Task AddAsync(ModelCost modelCost, CancellationToken cancellationToken = default)
    {
        _modelCosts[modelCost.Id.Value] = modelCost;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ModelCost modelCost, CancellationToken cancellationToken = default)
    {
        _modelCosts[modelCost.Id.Value] = modelCost;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryCostRecordRepository : ICostRecordRepository
{
    private readonly ConcurrentDictionary<Guid, CostRecord> _records = new();

    public Task<CostRecord?> GetByIdAsync(CostRecordId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        _records.TryGetValue(id.Value, out var record);
        return Task.FromResult(record?.TenantId == tenantId ? record : null);
    }

    public Task<IReadOnlyList<CostRecord>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var results = _records.Values.Where(r => r.TenantId == tenantId).OrderByDescending(r => r.RecordedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<CostRecord>>(results);
    }

    public Task<IReadOnlyList<CostRecord>> QueryAsync(TenantId tenantId, string? userId, string? departmentId, string? providerId, string? modelId, CostCategory? category, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _records.Values.Where(r => r.TenantId == tenantId);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(r => r.UserId == userId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(r => r.DepartmentId == departmentId);

        if (!string.IsNullOrEmpty(providerId))
            query = query.Where(r => r.ProviderId == providerId);

        if (!string.IsNullOrEmpty(modelId))
            query = query.Where(r => r.ModelId == modelId);

        if (category.HasValue)
            query = query.Where(r => r.Category == category.Value);

        if (from.HasValue)
            query = query.Where(r => r.RecordedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.RecordedAt <= to.Value);

        var results = query.OrderByDescending(r => r.RecordedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<CostRecord>>(results);
    }

    public Task<int> CountAsync(TenantId tenantId, string? userId, string? departmentId, string? providerId, string? modelId, CostCategory? category, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        var query = _records.Values.Where(r => r.TenantId == tenantId);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(r => r.UserId == userId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(r => r.DepartmentId == departmentId);

        if (!string.IsNullOrEmpty(providerId))
            query = query.Where(r => r.ProviderId == providerId);

        if (!string.IsNullOrEmpty(modelId))
            query = query.Where(r => r.ModelId == modelId);

        if (category.HasValue)
            query = query.Where(r => r.Category == category.Value);

        if (from.HasValue)
            query = query.Where(r => r.RecordedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.RecordedAt <= to.Value);

        return Task.FromResult(query.Count());
    }

    public Task AddAsync(CostRecord record, CancellationToken cancellationToken = default)
    {
        _records[record.Id.Value] = record;
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<CostRecord> records, CancellationToken cancellationToken = default)
    {
        foreach (var record in records)
            _records[record.Id.Value] = record;
        return Task.CompletedTask;
    }

    public Task<CostAggregation> GetAggregationAsync(TenantId tenantId, int year, int month, string? departmentId, string? userId, CancellationToken cancellationToken = default)
    {
        var query = _records.Values.Where(r =>
            r.TenantId == tenantId &&
            r.PeriodYear == year &&
            r.PeriodMonth == month);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(r => r.DepartmentId == departmentId);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(r => r.UserId == userId);

        var records = query.ToList();

        var totalCost = records.Sum(r => r.Amount.Value);
        var totalRequests = records.Count;
        var totalInputTokens = records.Sum(r => r.InputTokens);
        var totalOutputTokens = records.Sum(r => r.OutputTokens);

        var requestsByProvider = records.GroupBy(r => r.ProviderId).ToDictionary(g => g.Key, g => g.Count());
        var requestsByModel = records.GroupBy(r => r.ModelId).ToDictionary(g => g.Key, g => g.Count());
        var requestsByCategory = records.GroupBy(r => r.Category.ToString()).ToDictionary(g => g.Key, g => g.Count());

        var costByProvider = records.GroupBy(r => r.ProviderId).ToDictionary(g => g.Key, g => g.Sum(r => r.Amount.Value));
        var costByModel = records.GroupBy(r => r.ModelId).ToDictionary(g => g.Key, g => g.Sum(r => r.Amount.Value));
        var costByCategory = records.GroupBy(r => r.Category.ToString()).ToDictionary(g => g.Key, g => g.Sum(r => r.Amount.Value));

        var dailyBreakdown = records
            .GroupBy(r => r.PeriodDay)
            .Select(g => new DailyAggregation(g.Key, g.Sum(r => r.Amount.Value), g.Count(), g.Sum(r => r.TotalTokens)))
            .OrderBy(d => d.Day)
            .ToList();

        return Task.FromResult(new CostAggregation(
            totalCost,
            totalRequests,
            totalInputTokens,
            totalOutputTokens,
            requestsByProvider,
            requestsByModel,
            requestsByCategory,
            costByProvider,
            costByModel,
            costByCategory,
            dailyBreakdown));
    }
}

public sealed class InMemoryAlertRepository : IAlertRepository
{
    private readonly ConcurrentDictionary<Guid, Alert> _alerts = new();

    public Task<Alert?> GetByIdAsync(AlertId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        _alerts.TryGetValue(id.Value, out var alert);
        return Task.FromResult(alert?.TenantId == tenantId ? alert : null);
    }

    public Task<IReadOnlyList<Alert>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var results = _alerts.Values.Where(a => a.TenantId == tenantId).OrderByDescending(a => a.TriggeredAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<Alert>>(results);
    }

    public Task<IReadOnlyList<Alert>> QueryAsync(TenantId tenantId, string? departmentId, string? userId, AlertStatus? status, AlertSeverity? severity, string? type, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _alerts.Values.Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(a => a.DepartmentId == departmentId);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(a => a.UserId == userId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (severity.HasValue)
            query = query.Where(a => a.Severity >= severity.Value);

        if (!string.IsNullOrEmpty(type))
            query = query.Where(a => a.Type == type);

        if (from.HasValue)
            query = query.Where(a => a.TriggeredAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.TriggeredAt <= to.Value);

        var results = query.OrderByDescending(a => a.TriggeredAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<Alert>>(results);
    }

    public Task<int> CountAsync(TenantId tenantId, string? departmentId, string? userId, AlertStatus? status, AlertSeverity? severity, string? type, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        var query = _alerts.Values.Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrEmpty(departmentId))
            query = query.Where(a => a.DepartmentId == departmentId);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(a => a.UserId == userId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (severity.HasValue)
            query = query.Where(a => a.Severity >= severity.Value);

        if (!string.IsNullOrEmpty(type))
            query = query.Where(a => a.Type == type);

        if (from.HasValue)
            query = query.Where(a => a.TriggeredAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.TriggeredAt <= to.Value);

        return Task.FromResult(query.Count());
    }

    public Task<IReadOnlyList<Alert>> GetActiveAlertsAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var alerts = _alerts.Values
            .Where(a => a.TenantId == tenantId && a.Status == AlertStatus.Active)
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.TriggeredAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<Alert>>(alerts);
    }

    public Task AddAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        _alerts[alert.Id.Value] = alert;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        _alerts[alert.Id.Value] = alert;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryRoutingRuleRepository : IRoutingRuleRepository
{
    private readonly ConcurrentDictionary<Guid, RoutingRule> _rules = new();

    public Task<RoutingRule?> GetByIdAsync(RoutingRuleId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        _rules.TryGetValue(id.Value, out var rule);
        return Task.FromResult(rule?.TenantId == tenantId ? rule : null);
    }

    public Task<IReadOnlyList<RoutingRule>> GetAllAsync(TenantId tenantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var results = _rules.Values.Where(r => r.TenantId == tenantId).OrderByDescending(r => r.Priority).ThenByDescending(r => r.CreatedAt).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<RoutingRule>>(results);
    }

    public Task<IReadOnlyList<RoutingRule>> QueryAsync(TenantId tenantId, string? sourceModelId, string? targetModelId, string? triggerType, bool? isEnabled, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _rules.Values.Where(r => r.TenantId == tenantId);

        if (!string.IsNullOrEmpty(sourceModelId))
            query = query.Where(r => r.SourceModelId == sourceModelId);

        if (!string.IsNullOrEmpty(targetModelId))
            query = query.Where(r => r.TargetModelId == targetModelId);

        if (!string.IsNullOrEmpty(triggerType))
            query = query.Where(r => r.TriggerType == triggerType);

        if (isEnabled.HasValue)
            query = query.Where(r => r.IsEnabled == isEnabled.Value);

        var results = query.OrderByDescending(r => r.Priority).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<RoutingRule>>(results);
    }

    public Task<int> CountAsync(TenantId tenantId, string? sourceModelId, string? targetModelId, string? triggerType, bool? isEnabled, CancellationToken cancellationToken = default)
    {
        var query = _rules.Values.Where(r => r.TenantId == tenantId);

        if (!string.IsNullOrEmpty(sourceModelId))
            query = query.Where(r => r.SourceModelId == sourceModelId);

        if (!string.IsNullOrEmpty(targetModelId))
            query = query.Where(r => r.TargetModelId == targetModelId);

        if (!string.IsNullOrEmpty(triggerType))
            query = query.Where(r => r.TriggerType == triggerType);

        if (isEnabled.HasValue)
            query = query.Where(r => r.IsEnabled == isEnabled.Value);

        return Task.FromResult(query.Count());
    }

    public Task AddAsync(RoutingRule rule, CancellationToken cancellationToken = default)
    {
        _rules[rule.Id.Value] = rule;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RoutingRule rule, CancellationToken cancellationToken = default)
    {
        _rules[rule.Id.Value] = rule;
        return Task.CompletedTask;
    }
}

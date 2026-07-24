using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Domain;

public sealed class Alert : AggregateRoot<AlertId>
{
    public TenantId TenantId { get; }
    public AlertSeverity Severity { get; }
    public AlertStatus Status { get; private set; }
    public string Type { get; }
    public string Title { get; }
    public string Message { get; }
    public string? DepartmentId { get; }
    public string? UserId { get; }
    public string? BudgetId { get; }
    public decimal ThresholdValue { get; }
    public decimal CurrentValue { get; }
    public decimal PercentageUsed { get; }
    public DateTimeOffset TriggeredAt { get; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public string? AcknowledgedBy { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public string? ResolvedBy { get; private set; }
    public string? ResolutionNotes { get; private set; }

    internal Alert(
        AlertId id,
        TenantId tenantId,
        AlertSeverity severity,
        AlertStatus status,
        string type,
        string title,
        string message,
        string? departmentId,
        string? userId,
        string? budgetId,
        decimal thresholdValue,
        decimal currentValue,
        decimal percentageUsed,
        DateTimeOffset triggeredAt,
        DateTimeOffset? acknowledgedAt,
        string? acknowledgedBy,
        DateTimeOffset? resolvedAt,
        string? resolvedBy,
        string? resolutionNotes)
        : base(id)
    {
        TenantId = tenantId;
        Severity = severity;
        Status = status;
        Type = type;
        Title = title;
        Message = message;
        DepartmentId = departmentId;
        UserId = userId;
        BudgetId = budgetId;
        ThresholdValue = thresholdValue;
        CurrentValue = currentValue;
        PercentageUsed = percentageUsed;
        TriggeredAt = triggeredAt;
        AcknowledgedAt = acknowledgedAt;
        AcknowledgedBy = acknowledgedBy;
        ResolvedAt = resolvedAt;
        ResolvedBy = resolvedBy;
        ResolutionNotes = resolutionNotes;
    }

    public static Alert Create(
        TenantId tenantId,
        AlertSeverity severity,
        string type,
        string title,
        string message,
        string? departmentId = null,
        string? userId = null,
        string? budgetId = null,
        decimal thresholdValue = 80,
        decimal currentValue = 0,
        decimal percentageUsed = 0)
    {
        return new Alert(
            AlertId.Create(),
            tenantId,
            severity,
            AlertStatus.Active,
            type,
            title,
            message,
            departmentId,
            userId,
            budgetId,
            thresholdValue,
            currentValue,
            percentageUsed,
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            null,
            null);
    }

    public static Alert BudgetThresholdExceeded(
        TenantId tenantId,
        BudgetId budgetId,
        string budgetName,
        decimal percentageUsed,
        decimal threshold)
    {
        var severity = percentageUsed >= 100 ? AlertSeverity.Emergency :
                      percentageUsed >= 90 ? AlertSeverity.Critical :
                      AlertSeverity.Warning;

        return new Alert(
            AlertId.Create(),
            tenantId,
            severity,
            AlertStatus.Active,
            "BudgetExceeded",
            $"Budget '{budgetName}' {GetThresholdLabel(severity)}",
            $"Budget '{budgetName}' has exceeded {percentageUsed:F1}% of its allocated amount. Threshold: {threshold}%.",
            null,
            null,
            budgetId.Value.ToString(),
            threshold,
            100m,
            percentageUsed,
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            null,
            null);
    }

    public static Alert QuotaThresholdExceeded(
        TenantId tenantId,
        string quotaType,
        string quotaId,
        string name,
        decimal percentageUsed,
        decimal threshold)
    {
        var severity = percentageUsed >= 100 ? AlertSeverity.Emergency :
                      percentageUsed >= 90 ? AlertSeverity.Critical :
                      AlertSeverity.Warning;

        return new Alert(
            AlertId.Create(),
            tenantId,
            severity,
            AlertStatus.Active,
            "QuotaExceeded",
            $"{quotaType} quota '{name}' {GetThresholdLabel(severity)}",
            $"{quotaType} quota '{name}' has exceeded {percentageUsed:F1}% of its limit. Threshold: {threshold}%.",
            quotaType == "Department" ? quotaId : null,
            quotaType == "User" ? quotaId : null,
            null,
            threshold,
            100m,
            percentageUsed,
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            null,
            null);
    }

    public static Alert CostForecastAnomaly(
        TenantId tenantId,
        string forecastDetails,
        decimal projectedOverage)
    {
        return new Alert(
            AlertId.Create(),
            tenantId,
            AlertSeverity.Warning,
            AlertStatus.Active,
            "CostForecastAnomaly",
            "Unusual cost growth pattern detected",
            $"Cost forecast indicates potential overage: {forecastDetails}. Projected overage: {projectedOverage:C}",
            null,
            null,
            null,
            0,
            0,
            0,
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            null,
            null);
    }

    private static string GetThresholdLabel(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Warning => "approaching limit",
        AlertSeverity.Critical => "near limit",
        AlertSeverity.Emergency => "exceeded",
        _ => "at risk"
    };

    public void Acknowledge(string acknowledgedBy)
    {
        if (Status != AlertStatus.Active)
            throw new InvalidOperationException($"Cannot acknowledge an alert with status '{Status}'");
        Status = AlertStatus.Acknowledged;
        AcknowledgedAt = DateTimeOffset.UtcNow;
        AcknowledgedBy = acknowledgedBy;
    }

    public void Resolve(string resolvedBy, string? notes = null)
    {
        if (Status == AlertStatus.Resolved)
            throw new InvalidOperationException("Alert is already resolved");
        Status = AlertStatus.Resolved;
        ResolvedAt = DateTimeOffset.UtcNow;
        ResolvedBy = resolvedBy;
        ResolutionNotes = notes;
    }

    public bool IsActive => Status == AlertStatus.Active;
    public bool IsAcknowledged => Status == AlertStatus.Acknowledged;
    public bool IsResolved => Status == AlertStatus.Resolved;
    public bool IsCritical => Severity >= AlertSeverity.Critical;
    public bool IsEmergency => Severity == AlertSeverity.Emergency;
}

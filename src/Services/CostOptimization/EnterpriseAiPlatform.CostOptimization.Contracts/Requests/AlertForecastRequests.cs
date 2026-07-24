namespace EnterpriseAiPlatform.CostOptimization.Contracts.Requests;

public sealed record AcknowledgeAlertRequest(
    string AlertId,
    string AcknowledgedBy);

public sealed record ResolveAlertRequest(
    string AlertId,
    string ResolvedBy,
    string? Notes);

public sealed record QueryAlertsRequest(
    string? DepartmentId,
    string? UserId,
    string? Status,
    string? Severity,
    string? Type,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Skip,
    int Take);

public sealed record CreateRoutingRuleRequest(
    string Name,
    string Description,
    string TriggerType,
    string SourceModelId,
    string TargetModelId,
    decimal CostThreshold,
    decimal UsageThresholdPercent,
    bool IsAutomatic,
    int Priority,
    DateTimeOffset? ExpiresAt,
    Dictionary<string, string>? Conditions);

public sealed record UpdateRoutingRuleRequest(
    string? Name,
    string? Description,
    bool? IsEnabled,
    decimal? CostThreshold,
    decimal? UsageThresholdPercent,
    DateTimeOffset? ExpiresAt);

public sealed record TriggerRoutingRuleRequest(
    string RuleId,
    decimal CurrentCost,
    decimal UsagePercent);

public sealed record QueryRoutingRulesRequest(
    string? SourceModelId,
    string? TargetModelId,
    string? TriggerType,
    bool? IsEnabled,
    int Skip,
    int Take);

public sealed record GenerateForecastRequest(
    int Year,
    int Month,
    string? DepartmentId);

public sealed record GenerateReportRequest(
    int Year,
    int Month,
    string? DepartmentId,
    string? UserId,
    string ReportType,
    DateTimeOffset? From,
    DateTimeOffset? To);

public sealed record CreateAlertThresholdRequest(
    string Name,
    string Type,
    string EntityType,
    string? EntityId,
    decimal ThresholdPercent,
    string Severity,
    bool IsEnabled);

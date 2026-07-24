using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationCommands;

public sealed record CreateBudgetCommand(
    CreateBudgetRequest Request,
    Guid CorrelationId) : IRequest<Result<BudgetCommandResult>>;

public sealed record UpdateBudgetCommand(
    Guid BudgetId,
    UpdateBudgetRequest Request,
    Guid CorrelationId) : IRequest<Result<BudgetCommandResult>>;

public sealed record SuspendBudgetCommand(
    Guid BudgetId,
    string Reason,
    Guid CorrelationId) : IRequest<Result<BudgetCommandResult>>;

public sealed record ResumeBudgetCommand(
    Guid BudgetId,
    Guid CorrelationId) : IRequest<Result<BudgetCommandResult>>;

public sealed record CreateDepartmentQuotaCommand(
    CreateDepartmentQuotaRequest Request,
    Guid CorrelationId) : IRequest<Result<DepartmentQuotaCommandResult>>;

public sealed record UpdateDepartmentQuotaCommand(
    Guid QuotaId,
    UpdateDepartmentQuotaRequest Request,
    Guid CorrelationId) : IRequest<Result<DepartmentQuotaCommandResult>>;

public sealed record CreateUserQuotaCommand(
    CreateUserQuotaRequest Request,
    Guid CorrelationId) : IRequest<Result<UserQuotaCommandResult>>;

public sealed record UpdateUserQuotaCommand(
    Guid QuotaId,
    UpdateUserQuotaRequest Request,
    Guid CorrelationId) : IRequest<Result<UserQuotaCommandResult>>;

public sealed record RecordCostCommand(
    RecordCostRequest Request,
    Guid CorrelationId) : IRequest<Result<CostRecordCommandResult>>;

public sealed record CreateModelCostCommand(
    CreateModelCostRequest Request,
    Guid CorrelationId) : IRequest<Result<ModelCostCommandResult>>;

public sealed record UpdateModelCostCommand(
    Guid ModelCostId,
    UpdateModelCostRequest Request,
    Guid CorrelationId) : IRequest<Result<ModelCostCommandResult>>;

public sealed record AcknowledgeAlertCommand(
    AcknowledgeAlertRequest Request,
    Guid CorrelationId) : IRequest<Result<AlertCommandResult>>;

public sealed record ResolveAlertCommand(
    ResolveAlertRequest Request,
    Guid CorrelationId) : IRequest<Result<AlertCommandResult>>;

public sealed record CreateRoutingRuleCommand(
    CreateRoutingRuleRequest Request,
    Guid CorrelationId) : IRequest<Result<RoutingRuleCommandResult>>;

public sealed record UpdateRoutingRuleCommand(
    Guid RuleId,
    UpdateRoutingRuleRequest Request,
    Guid CorrelationId) : IRequest<Result<RoutingRuleCommandResult>>;

public sealed record TriggerRoutingRuleCommand(
    TriggerRoutingRuleRequest Request,
    Guid CorrelationId) : IRequest<Result<RoutingRuleTriggerCommandResult>>;

public sealed record DisableRoutingRuleCommand(
    Guid RuleId,
    Guid CorrelationId) : IRequest<Result<RoutingRuleCommandResult>>;

public sealed record EnableRoutingRuleCommand(
    Guid RuleId,
    Guid CorrelationId) : IRequest<Result<RoutingRuleCommandResult>>;

// Command Results
public sealed record BudgetCommandResult(
    string BudgetId,
    string Name,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    decimal UtilizationPercent,
    string Status);

public sealed record DepartmentQuotaCommandResult(
    string QuotaId,
    string DepartmentId,
    string DepartmentName,
    decimal MonthlyLimit,
    decimal MonthlyUsage,
    decimal RemainingAmount,
    decimal UtilizationPercent,
    bool IsActive);

public sealed record UserQuotaCommandResult(
    string QuotaId,
    string UserId,
    decimal MonthlyLimit,
    decimal MonthlyUsage,
    decimal RemainingAmount,
    decimal UtilizationPercent,
    bool IsActive);

public sealed record CostRecordCommandResult(
    string RecordId,
    string UserId,
    decimal Amount,
    string Currency,
    int InputTokens,
    int OutputTokens,
    DateTimeOffset RecordedAt);

public sealed record ModelCostCommandResult(
    string ModelCostId,
    string ProviderId,
    string ModelId,
    string ModelName,
    decimal InputCostPerToken,
    decimal OutputCostPerToken,
    decimal CostPerRequest,
    bool IsActive);

public sealed record AlertCommandResult(
    string AlertId,
    string Type,
    string Title,
    string Status);

public sealed record RoutingRuleCommandResult(
    string RuleId,
    string Name,
    string SourceModelId,
    string TargetModelId,
    bool IsEnabled);

public sealed record RoutingRuleTriggerCommandResult(
    string RuleId,
    string RuleName,
    string SourceModelId,
    string TargetModelId,
    bool WasTriggered,
    DateTimeOffset? TriggeredAt,
    string? Message);

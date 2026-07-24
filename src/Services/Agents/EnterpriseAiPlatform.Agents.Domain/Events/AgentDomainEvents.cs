using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Domain.Events;

public sealed record PlanCreatedEvent(PlanId PlanId, AgentId AgentId, TenantId TenantId) : IDomainEvent;
public sealed record StepStartedEvent(PlanId PlanId, StepId StepId, string StepName) : IDomainEvent;
public sealed record StepCompletedEvent(PlanId PlanId, StepId StepId, string Output) : IDomainEvent;
public sealed record StepFailedEvent(PlanId PlanId, StepId StepId, string ErrorMessage, int AttemptCount) : IDomainEvent;
public sealed record ApprovalRequestedEvent(ApprovalId ApprovalId, PlanId PlanId, StepId StepId, string ToolName, string ArgumentsJson) : IDomainEvent;
public sealed record ApprovalDecidedEvent(ApprovalId ApprovalId, PlanId PlanId, StepId StepId, bool IsApproved, string DecidedBy, string? Reason) : IDomainEvent;
public sealed record PlanCompletedEvent(PlanId PlanId, AgentId AgentId, bool Success) : IDomainEvent;

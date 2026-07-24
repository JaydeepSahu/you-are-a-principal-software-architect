using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Domain.Events;

public sealed record PlanCreatedEvent(PlanId PlanId, AgentId AgentId, TenantId TenantId) : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);
public sealed record StepStartedEvent(PlanId PlanId, StepId StepId, string StepName) : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);
public sealed record StepCompletedEvent(PlanId PlanId, StepId StepId, string Output) : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);
public sealed record StepFailedEvent(PlanId PlanId, StepId StepId, string ErrorMessage, int AttemptCount) : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);
public sealed record ApprovalRequestedEvent(ApprovalId ApprovalId, PlanId PlanId, StepId StepId, string ToolName, string ArgumentsJson) : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);
public sealed record ApprovalDecidedEvent(ApprovalId ApprovalId, PlanId PlanId, StepId StepId, bool IsApproved, string DecidedBy, string? Reason) : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);
public sealed record PlanCompletedEvent(PlanId PlanId, AgentId AgentId, bool Success) : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);

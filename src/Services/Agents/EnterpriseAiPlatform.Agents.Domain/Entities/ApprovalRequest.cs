using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Domain.Entities;

public sealed class ApprovalRequest : Entity<ApprovalId>
{
    public PlanId PlanId { get; }
    public StepId StepId { get; }
    public string ToolName { get; }
    public string ArgumentsJson { get; }
    public ApprovalStatus Status { get; private set; }
    public string? DecidedBy { get; private set; }
    public string? DecisionReason { get; private set; }
    public DateTimeOffset RequestedAt { get; }
    public DateTimeOffset? DecidedAt { get; private set; }

    public ApprovalRequest(
        ApprovalId id,
        PlanId planId,
        StepId stepId,
        string toolName,
        string argumentsJson)
        : base(id)
    {
        PlanId = planId;
        StepId = stepId;
        ToolName = toolName;
        ArgumentsJson = argumentsJson;
        Status = ApprovalStatus.Pending;
        RequestedAt = DateTimeOffset.UtcNow;
    }

    public void Approve(string decidedBy, string? reason = null)
    {
        Status = ApprovalStatus.Approved;
        DecidedBy = decidedBy;
        DecisionReason = reason;
        DecidedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string decidedBy, string? reason = null)
    {
        Status = ApprovalStatus.Rejected;
        DecidedBy = decidedBy;
        DecisionReason = reason;
        DecidedAt = DateTimeOffset.UtcNow;
    }

    public void Timeout()
    {
        Status = ApprovalStatus.TimedOut;
        DecidedAt = DateTimeOffset.UtcNow;
    }
}

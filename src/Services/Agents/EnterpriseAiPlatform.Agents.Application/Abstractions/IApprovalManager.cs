using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;

namespace EnterpriseAiPlatform.Agents.Application.Abstractions;

public interface IApprovalManager
{
    ApprovalRequest CreateRequest(PlanId planId, StepId stepId, string toolName, string argumentsJson);
    ApprovalRequest? GetRequest(ApprovalId approvalId);
    IReadOnlyCollection<ApprovalRequest> GetPendingRequestsForPlan(PlanId planId);
    bool SubmitDecision(ApprovalId approvalId, bool approve, string decidedBy, string? reason = null);
}

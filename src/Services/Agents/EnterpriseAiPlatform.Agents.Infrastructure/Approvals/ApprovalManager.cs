using System.Collections.Concurrent;
using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;

namespace EnterpriseAiPlatform.Agents.Infrastructure.Approvals;

public sealed class ApprovalManager : IApprovalManager
{
    private readonly ConcurrentDictionary<ApprovalId, ApprovalRequest> _requests = new();

    public ApprovalRequest CreateRequest(PlanId planId, StepId stepId, string toolName, string argumentsJson)
    {
        var approvalId = ApprovalId.New();
        var request = new ApprovalRequest(approvalId, planId, stepId, toolName, argumentsJson);
        _requests[approvalId] = request;
        return request;
    }

    public ApprovalRequest? GetRequest(ApprovalId approvalId)
    {
        return _requests.TryGetValue(approvalId, out var req) ? req : null;
    }

    public IReadOnlyCollection<ApprovalRequest> GetPendingRequestsForPlan(PlanId planId)
    {
        return _requests.Values
            .Where(r => r.PlanId == planId && r.Status == ApprovalStatus.Pending)
            .ToList()
            .AsReadOnly();
    }

    public bool SubmitDecision(ApprovalId approvalId, bool approve, string decidedBy, string? reason = null)
    {
        if (!_requests.TryGetValue(approvalId, out var req) || req.Status != ApprovalStatus.Pending)
        {
            return false;
        }

        if (approve)
        {
            req.Approve(decidedBy, reason);
        }
        else
        {
            req.Reject(decidedBy, reason);
        }

        return true;
    }
}

using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Contracts.Responses;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.Agents.Application.Commands;

public sealed class SubmitApprovalDecisionCommandHandler : IRequestHandler<SubmitApprovalDecisionCommand, Result<ApprovalRequestDto>>
{
    private readonly IApprovalManager _approvalManager;

    public SubmitApprovalDecisionCommandHandler(IApprovalManager approvalManager)
    {
        _approvalManager = approvalManager;
    }

    public Task<Result<ApprovalRequestDto>> Handle(SubmitApprovalDecisionCommand command, CancellationToken cancellationToken)
    {
        var approvalId = ApprovalId.From(command.Request.ApprovalId);
        var approval = _approvalManager.GetRequest(approvalId);

        if (approval == null)
        {
            return Task.FromResult(Result<ApprovalRequestDto>.Failure(new Error("Approval.NotFound", $"Approval request with ID {approvalId.Value} was not found.")));
        }

        var success = _approvalManager.SubmitDecision(approvalId, command.Request.Approve, command.Request.DecidedBy, command.Request.Reason);
        if (!success)
        {
            return Task.FromResult(Result<ApprovalRequestDto>.Failure(new Error("Approval.InvalidState", $"Approval request with ID {approvalId.Value} is already decided or invalid.")));
        }

        var dto = new ApprovalRequestDto(
            approval.Id.Value,
            approval.PlanId.Value,
            approval.StepId.Value,
            approval.ToolName,
            approval.ArgumentsJson,
            approval.Status.ToString(),
            approval.DecidedBy,
            approval.DecisionReason,
            approval.RequestedAt,
            approval.DecidedAt
        );

        return Task.FromResult(Result<ApprovalRequestDto>.Success(dto));
    }
}

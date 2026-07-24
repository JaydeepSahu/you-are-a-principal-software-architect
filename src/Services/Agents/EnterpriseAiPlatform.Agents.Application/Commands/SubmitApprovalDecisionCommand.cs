using EnterpriseAiPlatform.Agents.Contracts.Requests;
using EnterpriseAiPlatform.Agents.Contracts.Responses;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.Agents.Application.Commands;

public sealed record SubmitApprovalDecisionCommand(
    TenantId TenantId,
    ApprovalDecisionRequest Request) : IRequest<Result<ApprovalRequestDto>>;

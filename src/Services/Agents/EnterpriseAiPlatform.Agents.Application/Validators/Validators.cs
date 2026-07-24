using EnterpriseAiPlatform.Agents.Application.Commands;
using FluentValidation;

namespace EnterpriseAiPlatform.Agents.Application.Validators;

public sealed class ExecuteAgentPlanCommandValidator : AbstractValidator<ExecuteAgentPlanCommand>
{
    public ExecuteAgentPlanCommandValidator()
    {
        RuleFor(x => x.TenantId).NotNull().WithMessage("TenantId is required.");
        RuleFor(x => x.Request.AgentId).NotEmpty().WithMessage("AgentId is required.");
        RuleFor(x => x.Request.Goal).NotEmpty().WithMessage("Goal is required.");
    }
}

public sealed class SubmitApprovalDecisionCommandValidator : AbstractValidator<SubmitApprovalDecisionCommand>
{
    public SubmitApprovalDecisionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotNull().WithMessage("TenantId is required.");
        RuleFor(x => x.Request.ApprovalId).NotEmpty().WithMessage("ApprovalId is required.");
        RuleFor(x => x.Request.DecidedBy).NotEmpty().WithMessage("DecidedBy is required.");
    }
}

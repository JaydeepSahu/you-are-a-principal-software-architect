using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Contracts.Responses;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.Agents.Application.Queries;

public sealed class GetAgentPlanQueryHandler : IRequestHandler<GetAgentPlanQuery, Result<AgentExecutionResponse>>
{
    private readonly IApprovalManager _approvalManager;

    public GetAgentPlanQueryHandler(IApprovalManager approvalManager)
    {
        _approvalManager = approvalManager;
    }

    public Task<Result<AgentExecutionResponse>> Handle(GetAgentPlanQuery query, CancellationToken cancellationToken)
    {
        var planId = PlanId.From(query.PlanId);
        var pendingApproval = _approvalManager.GetPendingRequestsForPlan(planId).FirstOrDefault();

        // In production runtime, stored in persistence/EF Core. Return current status template.
        var response = new AgentExecutionResponse(
            planId.Value,
            "agent-runtime",
            "Executing",
            "Agent Plan Progress",
            new List<PlanStepDto>(),
            new Dictionary<string, string>(),
            pendingApproval?.Id.Value,
            DateTimeOffset.UtcNow,
            null
        );

        return Task.FromResult(Result<AgentExecutionResponse>.Success(response));
    }
}

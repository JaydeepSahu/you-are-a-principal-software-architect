using EnterpriseAiPlatform.Agents.Contracts.Requests;
using EnterpriseAiPlatform.Agents.Contracts.Responses;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Sdk.Client;

public interface IAgentClient
{
    Task<Result<AgentExecutionResponse>> RunAsync(
        TenantId tenantId,
        string goal,
        Dictionary<string, string>? initialWorkingMemory = null,
        CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> SubmitApprovalDecisionAsync(
        TenantId tenantId,
        Guid approvalId,
        bool approve,
        string decidedBy,
        string? reason = null,
        CancellationToken cancellationToken = default);
}

public sealed class LocalAgentClient : IAgentClient
{
    private readonly string _agentId;
    private readonly Application.Abstractions.IAgentPlanner _planner;
    private readonly Application.Abstractions.IAgentExecutor _executor;
    private readonly Application.Abstractions.IAgentMemoryStore _memoryStore;
    private readonly Application.Abstractions.IApprovalManager _approvalManager;

    public LocalAgentClient(
        string agentId,
        Application.Abstractions.IAgentPlanner planner,
        Application.Abstractions.IAgentExecutor executor,
        Application.Abstractions.IAgentMemoryStore memoryStore,
        Application.Abstractions.IApprovalManager approvalManager)
    {
        _agentId = agentId;
        _planner = planner;
        _executor = executor;
        _memoryStore = memoryStore;
        _approvalManager = approvalManager;
    }

    public async Task<Result<AgentExecutionResponse>> RunAsync(
        TenantId tenantId,
        string goal,
        Dictionary<string, string>? initialWorkingMemory = null,
        CancellationToken cancellationToken = default)
    {
        var agentId = Domain.ValueObjects.AgentId.From(_agentId);
        var planId = Domain.ValueObjects.PlanId.New();
        var memory = await _memoryStore.GetOrCreateAsync(planId, tenantId, agentId, cancellationToken);

        if (initialWorkingMemory != null)
        {
            foreach (var (k, v) in initialWorkingMemory)
            {
                memory.SetWorkingState(k, v);
            }
        }

        var planResult = await _planner.CreatePlanAsync(agentId, tenantId, goal, memory, cancellationToken);
        if (planResult.IsFailure)
        {
            return Result.Failure<AgentExecutionResponse>(planResult.Error);
        }

        var plan = planResult.Value;
        var execResult = await _executor.ExecutePlanAsync(plan, memory, null, cancellationToken);
        if (execResult.IsFailure)
        {
            return Result.Failure<AgentExecutionResponse>(execResult.Error);
        }

        await _memoryStore.SaveAsync(memory, cancellationToken);

        var pendingApproval = _approvalManager.GetPendingRequestsForPlan(plan.Id).FirstOrDefault();

        var stepDtos = plan.Steps.Select(s => new PlanStepDto(
            s.Id.Value,
            s.StepName,
            s.GoalDescription,
            s.ToolName,
            s.ArgumentsJson,
            s.Status.ToString(),
            s.Mode.ToString(),
            s.ExecutionGroup,
            s.DependsOnStepIds.Select(d => d.Value).ToList(),
            s.Output,
            s.ErrorMessage,
            s.AttemptCount,
            s.RequiresApproval,
            s.StartedAt,
            s.CompletedAt
        )).ToList();

        var response = new AgentExecutionResponse(
            plan.Id.Value,
            plan.AgentId.Value,
            plan.Status.ToString(),
            plan.UserGoal,
            stepDtos,
            memory.WorkingMemory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            pendingApproval?.Id.Value,
            plan.CreatedAt,
            plan.CompletedAt
        );

        return Result.Success(response);
    }

    public Task<Result<ApprovalRequestDto>> SubmitApprovalDecisionAsync(
        TenantId tenantId,
        Guid approvalId,
        bool approve,
        string decidedBy,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var appGuid = Domain.ValueObjects.ApprovalId.From(approvalId);
        var req = _approvalManager.GetRequest(appGuid);
        if (req == null)
        {
            return Task.FromResult(Result.Failure<ApprovalRequestDto>(new Error("Approval.NotFound", $"Approval request {approvalId} not found.")));
        }

        var success = _approvalManager.SubmitDecision(appGuid, approve, decidedBy, reason);
        if (!success)
        {
            return Task.FromResult(Result.Failure<ApprovalRequestDto>(new Error("Approval.InvalidState", $"Approval request {approvalId} is not pending.")));
        }

        var dto = new ApprovalRequestDto(
            req.Id.Value,
            req.PlanId.Value,
            req.StepId.Value,
            req.ToolName,
            req.ArgumentsJson,
            req.Status.ToString(),
            req.DecidedBy,
            req.DecisionReason,
            req.RequestedAt,
            req.DecidedAt
        );

        return Task.FromResult(Result.Success(dto));
    }
}

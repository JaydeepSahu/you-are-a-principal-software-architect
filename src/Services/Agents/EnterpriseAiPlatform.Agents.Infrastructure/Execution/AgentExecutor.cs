using System.Text.Json;
using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Contracts.Events;
using EnterpriseAiPlatform.Agents.Domain.Aggregates;
using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Infrastructure.Execution;

public sealed class AgentExecutor : IAgentExecutor
{
    private readonly IToolRegistry _toolRegistry;
    private readonly IApprovalManager _approvalManager;

    public AgentExecutor(
        IToolRegistry toolRegistry,
        IApprovalManager approvalManager)
    {
        _toolRegistry = toolRegistry;
        _approvalManager = approvalManager;
    }

    public async Task<Result<AgentPlan>> ExecutePlanAsync(
        AgentPlan plan,
        AgentMemory memory,
        Action<AgentStreamEventDto>? onStreamEvent = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(memory);

        plan.MarkExecuting();

        onStreamEvent?.Invoke(new AgentStreamEventDto(
            plan.Id.Value,
            AgentEventType.Thought,
            "Plan Execution Started",
            JsonSerializer.Serialize(new { Goal = plan.UserGoal, StepCount = plan.Steps.Count }),
            DateTimeOffset.UtcNow));

        while (plan.Status == PlanStatus.Executing)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var readySteps = plan.GetNextExecutableSteps().ToList();
            if (readySteps.Count == 0)
            {
                // Check if all steps are completed or skipped
                if (plan.Steps.All(s => s.Status == StepStatus.Completed || s.Status == StepStatus.Skipped))
                {
                    plan.MarkCompleted();
                    onStreamEvent?.Invoke(new AgentStreamEventDto(
                        plan.Id.Value,
                        AgentEventType.PlanCompleted,
                        "Plan completed successfully",
                        JsonSerializer.Serialize(new { Status = plan.Status.ToString() }),
                        DateTimeOffset.UtcNow));
                    return Result.Success(plan);
                }

                if (plan.Steps.Any(s => s.Status == StepStatus.WaitingForApproval))
                {
                    plan.MarkWaitingForApproval();
                    return Result.Success(plan); // Paused for human approval
                }

                if (plan.Steps.Any(s => s.Status == StepStatus.Failed))
                {
                    plan.MarkFailed();
                    return Result.Failure<AgentPlan>(new Error("Plan.StepFailed", "One or more plan steps failed during execution."));
                }

                break;
            }

            // Group steps by execution mode & group
            var parallelSteps = readySteps.Where(s => s.Mode == ExecutionMode.ParallelGroup).ToList();
            var sequentialSteps = readySteps.Where(s => s.Mode == ExecutionMode.Sequential).ToList();

            if (parallelSteps.Count > 0)
            {
                // Execute parallel steps concurrently using Task.WhenAll
                var tasks = parallelSteps.Select(step => ExecuteSingleStepAsync(plan, step, memory, onStreamEvent, cancellationToken));
                await Task.WhenAll(tasks);
            }

            foreach (var seqStep in sequentialSteps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteSingleStepAsync(plan, seqStep, memory, onStreamEvent, cancellationToken);
                if (seqStep.Status == StepStatus.WaitingForApproval)
                {
                    plan.MarkWaitingForApproval();
                    return Result<AgentPlan>.Success(plan);
                }
            }
        }

        return Result<AgentPlan>.Success(plan);
    }

    private async Task ExecuteSingleStepAsync(
        AgentPlan plan,
        PlanStep step,
        AgentMemory memory,
        Action<AgentStreamEventDto>? onStreamEvent,
        CancellationToken cancellationToken)
    {
        step.MarkRunning();

        onStreamEvent?.Invoke(new AgentStreamEventDto(
            plan.Id.Value,
            AgentEventType.StepStarted,
            $"Starting Step: {step.StepName}",
            JsonSerializer.Serialize(new { StepId = step.Id.Value, Tool = step.ToolName }),
            DateTimeOffset.UtcNow));

        // Check human-in-the-loop approval requirement
        if (step.RequiresApproval)
        {
            var existingApproval = _approvalManager.GetPendingRequestsForPlan(plan.Id).FirstOrDefault(a => a.StepId == step.Id);
            if (existingApproval == null)
            {
                var appReq = _approvalManager.CreateRequest(plan.Id, step.Id, step.ToolName ?? step.StepName, step.ArgumentsJson ?? "{}");
                step.MarkWaitingForApproval();

                onStreamEvent?.Invoke(new AgentStreamEventDto(
                    plan.Id.Value,
                    AgentEventType.ApprovalRequired,
                    $"Step '{step.StepName}' requires human approval",
                    JsonSerializer.Serialize(new { ApprovalId = appReq.Id.Value, Tool = step.ToolName }),
                    DateTimeOffset.UtcNow));

                return;
            }

            if (existingApproval.Status == ApprovalStatus.Pending)
            {
                step.MarkWaitingForApproval();
                return;
            }

            if (existingApproval.Status == ApprovalStatus.Rejected)
            {
                step.MarkFailed($"Approval rejected by {existingApproval.DecidedBy}: {existingApproval.DecisionReason}");
                onStreamEvent?.Invoke(new AgentStreamEventDto(
                    plan.Id.Value,
                    AgentEventType.StepFailed,
                    $"Step '{step.StepName}' rejected",
                    JsonSerializer.Serialize(new { Reason = existingApproval.DecisionReason }),
                    DateTimeOffset.UtcNow));
                return;
            }
        }

        // Execute tool or thought step with exponential retry policy
        var result = await RetryPolicyExecutor.ExecuteWithRetryAsync(async ct =>
        {
            if (string.IsNullOrWhiteSpace(step.ToolName))
            {
                // Thought / Analysis step
                var thoughtOutput = $"Executed thought: '{step.GoalDescription}' against memory context ({memory.Messages.Count} messages).";
                return Result<string>.Success(thoughtOutput);
            }

            return await _toolRegistry.ExecuteToolAsync(step.ToolName, step.ArgumentsJson ?? "{}", ct);
        }, step.MaxRetries, cancellationToken: cancellationToken);

        if (result.IsSuccess)
        {
            step.MarkCompleted(result.Value);
            memory.AddMessage("assistant", $"Step '{step.StepName}' completed: {result.Value}");
            memory.SetWorkingState($"step:{step.Id.Value}:output", result.Value);

            onStreamEvent?.Invoke(new AgentStreamEventDto(
                plan.Id.Value,
                AgentEventType.StepCompleted,
                $"Completed Step: {step.StepName}",
                JsonSerializer.Serialize(new { StepId = step.Id.Value, Output = result.Value }),
                DateTimeOffset.UtcNow));
        }
        else
        {
            step.MarkFailed(result.Error.Message);
            onStreamEvent?.Invoke(new AgentStreamEventDto(
                plan.Id.Value,
                AgentEventType.StepFailed,
                $"Failed Step: {step.StepName}",
                JsonSerializer.Serialize(new { StepId = step.Id.Value, Error = result.Error.Message }),
                DateTimeOffset.UtcNow));
        }
    }
}

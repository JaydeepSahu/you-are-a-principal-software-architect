using System.Text.Json;
using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Domain.Aggregates;
using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Infrastructure.Planning;

public sealed class AgentPlanner : IAgentPlanner
{
    private readonly IToolRegistry _toolRegistry;

    public AgentPlanner(IToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public Task<Result<AgentPlan>> CreatePlanAsync(
        AgentId agentId,
        TenantId tenantId,
        string goal,
        AgentMemory memory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(goal))
        {
            return Task.FromResult(Result<AgentPlan>.Failure(new Error("Plan.InvalidGoal", "Goal cannot be null or empty.")));
        }

        var planId = memory.Id;
        var plan = new AgentPlan(planId, agentId, tenantId, goal);
        var tools = _toolRegistry.GetAllTools();

        // Heuristic & tool-aware step builder
        var thoughtStepId = StepId.New();
        var thoughtStep = new PlanStep(
            thoughtStepId,
            "Analyze Goal & Memory Context",
            $"Decompose user goal '{goal}' and check initial memory.",
            toolName: null,
            argumentsJson: null,
            mode: ExecutionMode.Sequential,
            executionGroup: 0
        );
        plan.AddStep(thoughtStep);

        // If goal explicitly asks for parallel execution or tool orchestration
        var availableTools = tools.ToList();
        if (availableTools.Count > 0)
        {
            var parallelGroupId = StepId.New();
            var step1Id = StepId.New();
            var step2Id = StepId.New();

            var firstTool = availableTools[0];
            bool requiresApproval = firstTool.RequiresApproval;

            var toolStep = new PlanStep(
                step1Id,
                $"Execute Tool {firstTool.Name}",
                $"Invoke tool {firstTool.Name} to address goal.",
                toolName: firstTool.Name,
                argumentsJson: "{}",
                mode: ExecutionMode.Sequential,
                executionGroup: 1,
                dependsOnStepIds: new[] { thoughtStepId },
                requiresApproval: requiresApproval,
                maxRetries: firstTool.MaxRetries
            );
            plan.AddStep(toolStep);

            if (availableTools.Count > 1)
            {
                var secondTool = availableTools[1];
                var secondToolStep = new PlanStep(
                    step2Id,
                    $"Execute Tool {secondTool.Name} (Parallel)",
                    $"Invoke tool {secondTool.Name} concurrently.",
                    toolName: secondTool.Name,
                    argumentsJson: "{}",
                    mode: ExecutionMode.ParallelGroup,
                    executionGroup: 1,
                    dependsOnStepIds: new[] { thoughtStepId },
                    requiresApproval: secondTool.RequiresApproval,
                    maxRetries: secondTool.MaxRetries
                );
                plan.AddStep(secondToolStep);
            }
        }

        var finalSynthesisStep = new PlanStep(
            StepId.New(),
            "Synthesize Results",
            "Consolidate step outputs into final response payload.",
            toolName: null,
            argumentsJson: null,
            mode: ExecutionMode.Sequential,
            executionGroup: 2,
            dependsOnStepIds: plan.Steps.Select(s => s.Id).ToList()
        );
        plan.AddStep(finalSynthesisStep);

        return Task.FromResult(Result<AgentPlan>.Success(plan));
    }
}

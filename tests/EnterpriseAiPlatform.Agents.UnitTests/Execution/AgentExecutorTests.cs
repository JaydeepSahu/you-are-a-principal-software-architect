using EnterpriseAiPlatform.Agents.Domain.Aggregates;
using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.Agents.Infrastructure.Approvals;
using EnterpriseAiPlatform.Agents.Infrastructure.Execution;
using EnterpriseAiPlatform.Agents.Infrastructure.Tools;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Agents.UnitTests.Execution;

public class AgentExecutorTests
{
    [Fact]
    public async Task ExecutePlanAsync_ExecutesSequentialAndParallelStepsSuccessfully()
    {
        // Arrange
        var registry = new ToolRegistry();
        registry.RegisterTool(
            new ToolDefinition("tool_a", "Tool A", Array.Empty<ToolParameter>()),
            (args, ct) => Task.FromResult("output_a")
        );
        registry.RegisterTool(
            new ToolDefinition("tool_b", "Tool B", Array.Empty<ToolParameter>()),
            (args, ct) => Task.FromResult("output_b")
        );

        var approvalManager = new ApprovalManager();
        var executor = new AgentExecutor(registry, approvalManager);

        var planId = PlanId.New();
        var tenantId = TenantId.From("tenant-123");
        var agentId = AgentId.From("agent-abc");

        var plan = new AgentPlan(planId, agentId, tenantId, "Execute task");
        var step1 = new PlanStep(StepId.New(), "Step 1", "Run tool A", "tool_a", "{}", ExecutionMode.Sequential, 0);
        var step2 = new PlanStep(StepId.New(), "Step 2", "Run tool B", "tool_b", "{}", ExecutionMode.ParallelGroup, 1, new[] { step1.Id });

        plan.AddStep(step1);
        plan.AddStep(step2);

        var memory = new AgentMemory(planId, tenantId, agentId);

        // Act
        var result = await executor.ExecutePlanAsync(plan, memory);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlanStatus.Completed, plan.Status);
        Assert.All(plan.Steps, s => Assert.Equal(StepStatus.Completed, s.Status));
    }

    [Fact]
    public async Task ExecutePlanAsync_WithRequiredApproval_PausesExecutionAndCreatesApprovalRequest()
    {
        // Arrange
        var registry = new ToolRegistry();
        registry.RegisterTool(
            new ToolDefinition("deploy_tool", "Deploy to prod", Array.Empty<ToolParameter>(), requiresApproval: true),
            (args, ct) => Task.FromResult("deployed")
        );

        var approvalManager = new ApprovalManager();
        var executor = new AgentExecutor(registry, approvalManager);

        var planId = PlanId.New();
        var tenantId = TenantId.From("tenant-123");
        var agentId = AgentId.From("deployer-agent");

        var plan = new AgentPlan(planId, agentId, tenantId, "Deploy app");
        var step1 = new PlanStep(StepId.New(), "Deploy Step", "Run deploy", "deploy_tool", "{}", ExecutionMode.Sequential, 0, requiresApproval: true);
        plan.AddStep(step1);

        var memory = new AgentMemory(planId, tenantId, agentId);

        // Act
        var result = await executor.ExecutePlanAsync(plan, memory);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlanStatus.WaitingForApproval, plan.Status);
        Assert.Equal(StepStatus.WaitingForApproval, step1.Status);

        var pendingApprovals = approvalManager.GetPendingRequestsForPlan(planId);
        Assert.Single(pendingApprovals);
        Assert.Equal("deploy_tool", pendingApprovals.First().ToolName);
    }
}

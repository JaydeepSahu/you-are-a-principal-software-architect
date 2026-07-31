using EnterpriseAiPlatform.Agents.Domain.Aggregates;
using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.Agents.Infrastructure.Planning;
using EnterpriseAiPlatform.Agents.Infrastructure.Tools;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Agents.UnitTests.Planning;

public class AgentPlannerTests
{
    [Fact]
    public async Task CreatePlanAsync_WithValidGoalAndTools_GeneratesPlanWithDependenciesAndParallelGroups()
    {
        // Arrange
        var registry = new ToolRegistry();
        registry.RegisterTool(
            new ToolDefinition("search_docs", "Search documentation", Array.Empty<ToolParameter>()),
            (args, ct) => Task.FromResult("search_result")
        );
        registry.RegisterTool(
            new ToolDefinition("analyze_code", "Analyze codebase", Array.Empty<ToolParameter>()),
            (args, ct) => Task.FromResult("analysis_result")
        );

        var planner = new AgentPlanner(registry);
        var agentId = AgentId.From("code-architect-agent");
        var tenantId = TenantId.From(Guid.Parse("44444444-4444-4444-4444-444444444444"));
        var memory = new AgentMemory(PlanId.New(), tenantId, agentId);

        // Act
        var result = await planner.CreatePlanAsync(agentId, tenantId, "Review codebase architecture", memory);

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.Equal("code-architect-agent", plan.AgentId.Value);
        Assert.True(plan.Steps.Count >= 3);
        Assert.Contains(plan.Steps, s => s.Mode == ExecutionMode.ParallelGroup);
    }
}

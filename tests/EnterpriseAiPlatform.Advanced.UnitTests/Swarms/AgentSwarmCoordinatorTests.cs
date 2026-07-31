using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Infrastructure.Approvals;
using EnterpriseAiPlatform.Agents.Infrastructure.Execution;
using EnterpriseAiPlatform.Agents.Infrastructure.Memory;
using EnterpriseAiPlatform.Agents.Infrastructure.Swarms;
using EnterpriseAiPlatform.Agents.Infrastructure.Tools;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Advanced.UnitTests.Swarms;

public class AgentSwarmCoordinatorTests
{
    [Fact]
    public async Task ExecuteSwarmWorkflowAsync_RunsLeaderAndFollowerAgents()
    {
        // Arrange
        var registry = new ToolRegistry();
        var approvalManager = new ApprovalManager();
        var executor = new AgentExecutor(registry, approvalManager);
        var memoryStore = new InMemoryAgentMemoryStore();
        var coordinator = new AgentSwarmCoordinator(executor, memoryStore);

        var tenantId = TenantId.From(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var tasks = new List<SwarmTask>
        {
            new SwarmTask("task-1", "research-agent", "Research codebase", new List<string>()),
            new SwarmTask("task-2", "refactor-agent", "Refactor module", new List<string> { "task-1" })
        };

        // Act
        var result = await coordinator.ExecuteSwarmWorkflowAsync(tenantId, "lead-architect", "System Refactor Swarm", tasks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Success);
        Assert.Equal("lead-architect", result.Value.LeaderAgentId);
    }
}

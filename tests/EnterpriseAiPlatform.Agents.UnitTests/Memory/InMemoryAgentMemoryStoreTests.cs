using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.Agents.Infrastructure.Memory;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Agents.UnitTests.Memory;

public class InMemoryAgentMemoryStoreTests
{
    [Fact]
    public async Task GetOrCreateAsync_StoresAndRetrievesMemory()
    {
        // Arrange
        var store = new InMemoryAgentMemoryStore();
        var planId = PlanId.New();
        var tenantId = TenantId.From("t1");
        var agentId = AgentId.From("a1");

        // Act
        var mem1 = await store.GetOrCreateAsync(planId, tenantId, agentId);
        mem1.AddMessage("user", "Hello agent");
        mem1.SetWorkingState("status", "in-progress");
        await store.SaveAsync(mem1);

        var mem2 = await store.GetOrCreateAsync(planId, tenantId, agentId);

        // Assert
        Assert.Single(mem2.Messages);
        Assert.Equal("in-progress", mem2.GetWorkingState("status"));
    }
}

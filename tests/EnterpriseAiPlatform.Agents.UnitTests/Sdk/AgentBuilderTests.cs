using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Sdk.Builder;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Agents.UnitTests.Sdk;

public class AgentBuilderTests
{
    [Fact]
    public async Task FluentAgentBuilder_BuildsAndRunsAgentWorkflow()
    {
        // Arrange
        var client = new AgentBuilder("code-reviewer")
            .AddTool(tb => tb
                .WithDescription("Runs static analysis")
                .WithParameter("target", ToolParameterType.String, "Target file")
                .OnExecute((args, ct) => Task.FromResult("Static analysis passed clean."))
            )
            .Build();

        var tenantId = TenantId.From("tenant-test");

        // Act
        var result = await client.RunAsync(tenantId, "Perform code review");

        // Assert
        Assert.True(result.IsSuccess);
        var response = result.Value;
        Assert.Equal("code-reviewer", response.AgentId);
        Assert.Equal("Completed", response.Status);
    }
}

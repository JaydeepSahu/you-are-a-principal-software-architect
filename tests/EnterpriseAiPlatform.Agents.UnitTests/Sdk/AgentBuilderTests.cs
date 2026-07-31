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
                .WithParameter("target", ToolParameterType.String, "Target file", isRequired: false)
                .OnExecute((args, ct) => Task.FromResult("Static analysis passed clean."))
            )
            .Build();

        var tenantId = TenantId.From(Guid.Parse("33333333-3333-3333-3333-333333333333"));

        // Act
        var result = await client.RunAsync(tenantId, "Perform code review");

        // Assert
        Assert.True(result.IsSuccess);
        var response = result.Value;
        Assert.Equal("code-reviewer", response.AgentId);
        Assert.Equal("Completed", response.Status);
    }
}

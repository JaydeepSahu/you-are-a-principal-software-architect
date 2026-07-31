using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.Agents.Infrastructure.Tools;
using Xunit;

namespace EnterpriseAiPlatform.Agents.UnitTests.Tools;

public class ToolRegistryTests
{
    [Fact]
    public async Task RegisterAndExecuteToolAsync_ValidatesParametersAndExecutesHandler()
    {
        // Arrange
        var registry = new ToolRegistry();
        var toolDef = new ToolDefinition(
            "calculator",
            "Perform calculation",
            new[] { new ToolParameter("expression", ToolParameterType.String, "Math expression", IsRequired: true) }
        );

        registry.RegisterTool(toolDef, (argsJson, ct) => Task.FromResult("42"));

        // Act - Valid Execution
        var result = await registry.ExecuteToolAsync("calculator", "{\"expression\":\"21 * 2\"}");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("42", result.Value);

        // Act - Missing required parameter
        var invalidResult = await registry.ExecuteToolAsync("calculator", "{}");
        Assert.True(invalidResult.IsFailure);
        Assert.Equal("Tool.InvalidArguments", invalidResult.Error.Code);
    }
}

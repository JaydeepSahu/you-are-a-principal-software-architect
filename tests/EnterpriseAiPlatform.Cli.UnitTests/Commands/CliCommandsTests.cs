using EnterpriseAiPlatform.Cli.Commands;
using Xunit;

namespace EnterpriseAiPlatform.Cli.UnitTests.Commands;

public class CliCommandsTests
{
    [Fact]
    public async Task ExecutePromptAsync_ReturnsSuccessExitCode()
    {
        // Act
        int exitCode = await CliCommands.ExecutePromptAsync("Test prompt");

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task RunAgentAsync_ReturnsSuccessExitCode()
    {
        // Act
        int exitCode = await CliCommands.RunAgentAsync("Refactor module");

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ShowStatusAsync_ReturnsSuccessExitCode()
    {
        // Act
        int exitCode = await CliCommands.ShowStatusAsync();

        // Assert
        Assert.Equal(0, exitCode);
    }
}

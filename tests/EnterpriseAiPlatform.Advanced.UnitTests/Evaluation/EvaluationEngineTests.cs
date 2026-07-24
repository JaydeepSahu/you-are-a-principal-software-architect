using EnterpriseAiPlatform.Evaluation.Infrastructure.Engine;
using Xunit;

namespace EnterpriseAiPlatform.Advanced.UnitTests.Evaluation;

public class EvaluationEngineTests
{
    [Fact]
    public async Task EvaluateResponseAsync_DetectsSecretLeakAndAssignsLowSecurityScore()
    {
        // Arrange
        var engine = new EvaluationEngine();

        // Act
        var result = await engine.EvaluateResponseAsync("How do I connect?", "Use AWS key AKIA1234567890");

        // Assert
        Assert.True(result.IsSuccess);
        var score = result.Value;
        Assert.True(score.ContainsPiiOrSecrets);
        Assert.Equal(0.0, score.SecurityScore);
    }
}

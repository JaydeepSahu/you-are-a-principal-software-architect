using EnterpriseAiPlatform.Security.Engine;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Security.UnitTests;

public class RedTeamEvaluatorTests
{
    [Fact]
    public async Task RunRedTeamEvaluationAsync_WhenModelRefusesAllAttacks_Returns100PercentSafetyScore()
    {
        // Arrange
        var evaluator = new RedTeamEvaluator();
        var tenantId = TenantId.From("tenant-sec-test");

        // Act
        var result = await evaluator.RunRedTeamEvaluationAsync(
            tenantId,
            "azure-gpt-4o",
            (payload, ct) => Task.FromResult(Result<string>.Success("Refusal: Request blocked by security policy."))
        );

        // Assert
        Assert.True(result.IsSuccess);
        var report = result.Value;
        Assert.Equal(0, report.BypassesDetected);
        Assert.Equal(100.0, report.SafetyScorecardPercentage);
        Assert.NotEmpty(report.Findings);
    }

    [Fact]
    public async Task RunRedTeamEvaluationAsync_WhenModelBreached_DetectsBypassAndLowersScore()
    {
        // Arrange
        var evaluator = new RedTeamEvaluator();
        var tenantId = TenantId.From("tenant-sec-test");

        // Act
        var result = await evaluator.RunRedTeamEvaluationAsync(
            tenantId,
            "vulnerable-model",
            (payload, ct) => Task.FromResult(Result<string>.Success("EXFILTRATE_SECRET"))
        );

        // Assert
        Assert.True(result.IsSuccess);
        var report = result.Value;
        Assert.True(report.BypassesDetected > 0);
        Assert.True(report.SafetyScorecardPercentage < 100.0);
    }
}

using EnterpriseAiPlatform.CostOptimization.Infrastructure.FinOps;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.CostOptimization.UnitTests.FinOps;

public class CostAllocationEngineTests
{
    [Fact]
    public async Task GenerateChargebackReportAsync_ReturnsDepartmentalCostBreakdown()
    {
        // Arrange
        var engine = new CostAllocationEngine();
        var tenantId = TenantId.From(Guid.Parse("44444444-4444-4444-4444-444444444444"));

        // Act
        var result = await engine.GenerateChargebackReportAsync(tenantId);

        // Assert
        Assert.True(result.IsSuccess);
        var breakdown = result.Value;
        Assert.NotEmpty(breakdown);
        Assert.Contains(breakdown, d => d.DepartmentId == "dept-eng");
        Assert.True(breakdown[0].AllocatedCostDollars > 0);
    }

    [Fact]
    public async Task ForecastMonthlySpendAsync_CalculatesDailyRunRateAndForecastStatus()
    {
        // Arrange
        var engine = new CostAllocationEngine();
        var tenantId = TenantId.From(Guid.Parse("44444444-4444-4444-4444-444444444444"));

        // Act
        var result = await engine.ForecastMonthlySpendAsync(tenantId);

        // Assert
        Assert.True(result.IsSuccess);
        var forecast = result.Value;
        Assert.True(forecast.DailyRunRateDollars > 0);
        Assert.True(forecast.ProjectedEndOfMonthSpendDollars > 0);
        Assert.False(string.IsNullOrWhiteSpace(forecast.ForecastStatus));
    }
}

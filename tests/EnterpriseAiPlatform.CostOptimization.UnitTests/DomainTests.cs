using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;
using FluentAssertions;
using Xunit;

namespace EnterpriseAiPlatform.CostOptimization.UnitTests.Domain;

public class BudgetTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    [Fact]
    public void Create_WithValidData_ShouldCreateBudget()
    {
        // Arrange
        var allocatedAmount = CostAmount.Create(10000, "USD");

        // Act
        var budget = Budget.Create(
            TestTenantId,
            "Engineering Q1 Budget",
            "Q1 2024 engineering costs",
            allocatedAmount,
            BudgetPeriod.Quarterly,
            2024,
            1,
            1,
            null,
            null);

        // Assert
        budget.Should().NotBeNull();
        budget.Name.Should().Be("Engineering Q1 Budget");
        budget.AllocatedAmount.Value.Should().Be(10000);
        budget.SpentAmount.Value.Should().Be(0);
        budget.Status.Should().Be(BudgetStatus.Active);
        budget.Period.Should().Be(BudgetPeriod.Quarterly);
    }

    [Fact]
    public void AddSpending_ShouldUpdateSpentAmountAndStatus()
    {
        // Arrange
        var budget = Budget.Create(
            TestTenantId,
            "Test Budget",
            null,
            CostAmount.Create(1000, "USD"),
            BudgetPeriod.Monthly,
            2024,
            1,
            null,
            null,
            null);

        // Act
        budget.AddSpending(CostAmount.Create(500, "USD"));
        budget.AddSpending(CostAmount.Create(200, "USD"));

        // Assert
        budget.SpentAmount.Value.Should().Be(700);
        budget.RemainingAmount.Value.Should().Be(300);
        budget.UtilizationPercentage.Should().Be(70);
        budget.Status.Should().Be(BudgetStatus.Active);
    }

    [Fact]
    public void AddSpending_WhenNearLimit_ShouldSetNearLimitStatus()
    {
        // Arrange
        var budget = Budget.Create(
            TestTenantId,
            "Test Budget",
            null,
            CostAmount.Create(1000, "USD"),
            BudgetPeriod.Monthly,
            2024,
            1,
            null,
            null,
            null);

        // Act
        budget.AddSpending(CostAmount.Create(850, "USD"));

        // Assert
        budget.Status.Should().Be(BudgetStatus.NearLimit);
        budget.UtilizationPercentage.Should().Be(85);
    }

    [Fact]
    public void AddSpending_WhenExceeded_ShouldSetExceededStatus()
    {
        // Arrange
        var budget = Budget.Create(
            TestTenantId,
            "Test Budget",
            null,
            CostAmount.Create(1000, "USD"),
            BudgetPeriod.Monthly,
            2024,
            1,
            null,
            null,
            null);

        // Act
        budget.AddSpending(CostAmount.Create(1000, "USD"));

        // Assert
        budget.Status.Should().Be(BudgetStatus.Exceeded);
        budget.UtilizationPercentage.Should().Be(100);
    }

    [Fact]
    public void Suspend_ShouldUpdateStatusToSuspended()
    {
        // Arrange
        var budget = Budget.Create(
            TestTenantId,
            "Test Budget",
            null,
            CostAmount.Create(1000, "USD"),
            BudgetPeriod.Monthly,
            2024,
            1,
            null,
            null,
            null);

        // Act
        budget.Suspend("Budget limit reached");

        // Assert
        budget.Status.Should().Be(BudgetStatus.Suspended);
    }

    [Fact]
    public void AddSpending_WhenSuspended_ShouldThrowException()
    {
        // Arrange
        var budget = Budget.Create(
            TestTenantId,
            "Test Budget",
            null,
            CostAmount.Create(1000, "USD"),
            BudgetPeriod.Monthly,
            2024,
            1,
            null,
            null,
            null);
        budget.Suspend("Test");

        // Act & Assert
        var act = () => budget.AddSpending(CostAmount.Create(100, "USD"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateAllocation_ShouldUpdateAmount()
    {
        // Arrange
        var budget = Budget.Create(
            TestTenantId,
            "Test Budget",
            null,
            CostAmount.Create(1000, "USD"),
            BudgetPeriod.Monthly,
            2024,
            1,
            null,
            null,
            null);

        // Act
        budget.UpdateAllocation(CostAmount.Create(2000, "USD"));

        // Assert
        budget.AllocatedAmount.Value.Should().Be(2000);
        budget.RemainingAmount.Value.Should().Be(2000);
    }

    [Fact]
    public void Resume_WhenSuspended_ShouldRestoreActiveStatus()
    {
        // Arrange
        var budget = Budget.Create(
            TestTenantId,
            "Test Budget",
            null,
            CostAmount.Create(1000, "USD"),
            BudgetPeriod.Monthly,
            2024,
            1,
            null,
            null,
            null);
        budget.Suspend("Test");

        // Act
        budget.Resume();
        budget.AddSpending(CostAmount.Create(500, "USD"));

        // Assert
        budget.Status.Should().Be(BudgetStatus.Active);
    }
}

public class UserQuotaTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    [Fact]
    public void Create_WithValidData_ShouldCreateUserQuota()
    {
        // Arrange & Act
        var quota = UserQuota.Create(
            TestTenantId,
            "user-123",
            "dept-456",
            CostAmount.Create(500, "USD"),
            1000,
            null);

        // Assert
        quota.Should().NotBeNull();
        quota.UserId.Should().Be("user-123");
        quota.DepartmentId.Should().Be("dept-456");
        quota.MonthlyLimit.Value.Should().Be(500);
        quota.MonthlyUsage.Value.Should().Be(0);
        quota.MaxRequestsPerDay.Should().Be(1000);
        quota.RequestsToday.Should().Be(0);
        quota.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AddUsage_ShouldUpdateMonthlyUsage()
    {
        // Arrange
        var quota = UserQuota.Create(
            TestTenantId,
            "user-123",
            null,
            CostAmount.Create(500, "USD"),
            1000,
            null);

        // Act
        quota.AddUsage(CostAmount.Create(50, "USD"));
        quota.AddUsage(CostAmount.Create(25, "USD"));

        // Assert
        quota.MonthlyUsage.Value.Should().Be(75);
        quota.RemainingAmount.Value.Should().Be(425);
        quota.UtilizationPercentage.Should().Be(15);
    }

    [Fact]
    public void IncrementRequests_ShouldIncrementCounter()
    {
        // Arrange
        var quota = UserQuota.Create(
            TestTenantId,
            "user-123",
            null,
            CostAmount.Create(500, "USD"),
            100,
            null);

        // Act
        for (int i = 0; i < 50; i++)
            quota.IncrementRequests();

        // Assert
        quota.RequestsToday.Should().Be(50);
        quota.RemainingRequests.Should().Be(50);
        quota.CanMakeRequest().Should().BeTrue();
    }

    [Fact]
    public void IncrementRequests_WhenLimitReached_ShouldThrowException()
    {
        // Arrange
        var quota = UserQuota.Create(
            TestTenantId,
            "user-123",
            null,
            CostAmount.Create(500, "USD"),
            10,
            null);

        for (int i = 0; i < 10; i++)
            quota.IncrementRequests();

        // Act & Assert
        var act = () => quota.IncrementRequests();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var quota = UserQuota.Create(
            TestTenantId,
            "user-123",
            null,
            CostAmount.Create(500, "USD"),
            1000,
            null);

        // Act
        quota.Deactivate();

        // Assert
        quota.IsActive.Should().BeFalse();
    }

    [Fact]
    public void AddUsage_WhenInactive_ShouldThrowException()
    {
        // Arrange
        var quota = UserQuota.Create(
            TestTenantId,
            "user-123",
            null,
            CostAmount.Create(500, "USD"),
            1000,
            null);
        quota.Deactivate();

        // Act & Assert
        var act = () => quota.AddUsage(CostAmount.Create(50, "USD"));
        act.Should().Throw<InvalidOperationException>();
    }
}

public class CostAmountTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCostAmount()
    {
        // Act
        var amount = CostAmount.Create(99.99m, "USD");

        // Assert
        amount.Value.Should().Be(99.99m);
        amount.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeValue_ShouldThrowException()
    {
        // Act & Assert
        var act = () => CostAmount.Create(-10m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithInvalidCurrency_ShouldThrowException()
    {
        // Act & Assert
        var act = () => CostAmount.Create(100m, "US");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_ShouldSumAmounts()
    {
        // Arrange
        var amount1 = CostAmount.Create(50m, "USD");
        var amount2 = CostAmount.Create(30m, "USD");

        // Act
        var result = amount1.Add(amount2);

        // Assert
        result.Value.Should().Be(80m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ShouldThrowException()
    {
        // Arrange
        var amount1 = CostAmount.Create(50m, "USD");
        var amount2 = CostAmount.Create(30m, "EUR");

        // Act & Assert
        var act = () => amount1.Add(amount2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_ShouldReturnDifference()
    {
        // Arrange
        var amount1 = CostAmount.Create(100m, "USD");
        var amount2 = CostAmount.Create(30m, "USD");

        // Act
        var result = amount1.Subtract(amount2);

        // Assert
        result.Value.Should().Be(70m);
    }

    [Fact]
    public void Multiply_ShouldReturnScaledAmount()
    {
        // Arrange
        var amount = CostAmount.Create(100m, "USD");

        // Act
        var result = amount.Multiply(1.5m);

        // Assert
        result.Value.Should().Be(150m);
    }

    [Fact]
    public void AsPercentageOf_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var amount = CostAmount.Create(50m, "USD");
        var total = CostAmount.Create(200m, "USD");

        // Act
        var percentage = amount.AsPercentageOf(total);

        // Assert
        percentage.Should().Be(25m);
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        // Act
        var zero = CostAmount.Zero("USD");

        // Assert
        zero.Value.Should().Be(0);
        zero.Currency.Should().Be("USD");
        zero.IsZero.Should().BeTrue();
    }
}

public class ModelCostTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    [Fact]
    public void Create_WithValidData_ShouldCreateModelCost()
    {
        // Act
        var modelCost = ModelCost.Create(
            TestTenantId,
            "openai",
            "gpt-4",
            "GPT-4",
            0.00003m,
            0.00006m,
            0m,
            0m,
            DateTimeOffset.UtcNow);

        // Assert
        modelCost.Should().NotBeNull();
        modelCost.ProviderId.Should().Be("openai");
        modelCost.ModelId.Should().Be("gpt-4");
        modelCost.InputCostPerToken.Value.Should().Be(0.00003m);
        modelCost.OutputCostPerToken.Value.Should().Be(0.00006m);
        modelCost.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CalculateCost_WithTokensOnly_ShouldCalculateCorrectly()
    {
        // Arrange
        var modelCost = ModelCost.Create(
            TestTenantId,
            "openai",
            "gpt-4",
            "GPT-4",
            0.00003m,
            0.00006m,
            0m,
            0m,
            DateTimeOffset.UtcNow);

        // Act
        var cost = modelCost.CalculateCost(1000, 500, null);

        // Assert
        // 1000 * 0.00003 + 500 * 0.00006 = 0.03 + 0.03 = 0.06
        cost.Value.Should().Be(0.06m);
    }

    [Fact]
    public void CalculateCost_WithProcessingTime_ShouldIncludeComputeCost()
    {
        // Arrange
        var modelCost = ModelCost.Create(
            TestTenantId,
            "anthropic",
            "claude-3",
            "Claude 3",
            0.000015m,
            0.000075m,
            0m,
            0.0001m,
            DateTimeOffset.UtcNow);

        // Act
        var cost = modelCost.CalculateCost(1000, 500, 2.0);

        // Assert
        // Token costs: 1000 * 0.000015 + 500 * 0.000075 = 0.015 + 0.0375 = 0.0525
        // Compute cost: 2.0 * 0.0001 = 0.0002
        // Total: 0.0527
        cost.Value.Should().Be(0.0527m);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var modelCost = ModelCost.Create(
            TestTenantId,
            "openai",
            "gpt-4",
            "GPT-4",
            0.00003m,
            0.00006m,
            0m,
            0m,
            DateTimeOffset.UtcNow);

        // Act
        modelCost.Deactivate(DateTimeOffset.UtcNow.AddMonths(1));

        // Assert
        modelCost.IsActive.Should().BeFalse();
        modelCost.EffectiveTo.Should().NotBeNull();
    }
}

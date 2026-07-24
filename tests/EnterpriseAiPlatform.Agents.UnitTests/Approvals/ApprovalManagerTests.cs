using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.Agents.Infrastructure.Approvals;
using Xunit;

namespace EnterpriseAiPlatform.Agents.UnitTests.Approvals;

public class ApprovalManagerTests
{
    [Fact]
    public void CreateRequest_AndSubmitDecision_UpdatesStatusCorrectly()
    {
        // Arrange
        var manager = new ApprovalManager();
        var planId = PlanId.New();
        var stepId = StepId.New();

        // Act
        var req = manager.CreateRequest(planId, stepId, "database_migration", "{\"schema\":\"v2\"}");

        // Assert initial
        Assert.Equal(ApprovalStatus.Pending, req.Status);
        Assert.Single(manager.GetPendingRequestsForPlan(planId));

        // Act decision
        var success = manager.SubmitDecision(req.Id, approve: true, decidedBy: "lead.architect@company.com", reason: "Reviewed migration script");

        // Assert decision
        Assert.True(success);
        Assert.Equal(ApprovalStatus.Approved, req.Status);
        Assert.Equal("lead.architect@company.com", req.DecidedBy);
        Assert.Empty(manager.GetPendingRequestsForPlan(planId));
    }
}

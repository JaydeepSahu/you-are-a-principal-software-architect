using EnterpriseAiPlatform.Policy.Infrastructure.Dlp;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Policy.UnitTests.Dlp;

public class InlineDlpScannerTests
{
    [Fact]
    public async Task ScanAndRedactAsync_RedactsAwsKeyAndEmailAddress()
    {
        // Arrange
        var scanner = new InlineDlpScanner();
        var tenantId = TenantId.From("tenant-sec");
        var promptText = "Connect using key AKIAIOSFODNN7EXAMPLE and notify user john.doe@company.com";

        // Act
        var result = await scanner.ScanAndRedactAsync(tenantId, promptText);

        // Assert
        Assert.True(result.IsSuccess);
        var res = result.Value;
        Assert.False(res.ShouldBlock);
        Assert.Equal(2, res.Matches.Count);
        Assert.Contains("[REDACTED_AWS_KEY]", res.SanitizedText);
        Assert.Contains("[REDACTED_EMAIL]", res.SanitizedText);
    }

    [Fact]
    public async Task ScanAndRedactAsync_WithGitHubPatToken_FlagsShouldBlock()
    {
        // Arrange
        var scanner = new InlineDlpScanner();
        var tenantId = TenantId.From("tenant-sec");
        var promptText = "Push code using token ghp_123456789012345678901234567890123456";

        // Act
        var result = await scanner.ScanAndRedactAsync(tenantId, promptText);

        // Assert
        Assert.True(result.IsSuccess);
        var res = result.Value;
        Assert.True(res.ShouldBlock);
        Assert.NotNull(res.BlockReason);
    }
}

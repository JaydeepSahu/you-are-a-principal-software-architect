using EnterpriseAiPlatform.Agents.Infrastructure.Execution;
using EnterpriseAiPlatform.SharedKernel;
using Xunit;

namespace EnterpriseAiPlatform.Agents.UnitTests.Execution;

public class RetryPolicyExecutorTests
{
    [Fact]
    public async Task ExecuteWithRetryAsync_RetriesOnFailureAndSucceedsWhenAttemptSucceeds()
    {
        // Arrange
        int attemptCount = 0;

        // Act
        var result = await RetryPolicyExecutor.ExecuteWithRetryAsync<string>(
            ct =>
            {
                attemptCount++;
                if (attemptCount < 2)
                {
                    return Task.FromResult(Result<string>.Failure(new Error("Transient.Error", "Temporary network failure")));
                }
                return Task.FromResult(Result<string>.Success("SuccessPayload"));
            },
            maxRetries: 3
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("SuccessPayload", result.Value);
        Assert.Equal(2, attemptCount);
    }
}

using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.SharedKernel.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void SuccessCreatesSuccessfulResultWithoutError()
    {
        Result result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(ErrorDetail.None, result.Error);
    }

    [Fact]
    public void FailureRequiresExplicitError()
    {
        ErrorDetail error = ErrorDetail.Create("tenant.not_found", "Tenant was not found.");

        Result result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void GenericSuccessExposesValue()
    {
        Result<string> result = Result.Success("accepted");

        Assert.True(result.IsSuccess);
        Assert.Equal("accepted", result.Value);
    }

    [Fact]
    public void FailedGenericResultDoesNotExposeValue()
    {
        Result<string> result = Result.Failure<string>(
            ErrorDetail.Create("policy.denied", "Policy denied the request."));

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}

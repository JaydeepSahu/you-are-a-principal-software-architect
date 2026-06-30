using EnterpriseAiPlatform.Identity.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.Identity.UnitTests;

public sealed class RefreshTokenProtectorTests
{
    [Fact]
    public void RefreshTokenHashCanBeVerifiedWithoutStoringToken()
    {
        RefreshTokenProtector protector = new(Options.Create(new RefreshTokenSecurityOptions
        {
            Pepper = "refresh-token-test-pepper-with-enough-entropy",
            TokenBytes = 64
        }));
        string refreshToken = protector.Generate();

        string hash = protector.Hash(refreshToken);

        Assert.NotEqual(refreshToken, hash);
        Assert.True(protector.Verify(refreshToken, hash));
        Assert.False(protector.Verify($"{refreshToken}x", hash));
    }
}

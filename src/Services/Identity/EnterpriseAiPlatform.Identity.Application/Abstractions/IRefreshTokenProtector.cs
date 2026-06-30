namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public interface IRefreshTokenProtector
{
    string Generate();

    string Hash(string refreshToken);

    bool Verify(string refreshToken, string expectedHash);
}

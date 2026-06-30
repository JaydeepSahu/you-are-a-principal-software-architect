namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public interface IApiKeyHasher
{
    string Hash(Guid apiKeyId, string secret);

    bool Verify(Guid apiKeyId, string secret, string expectedHash);
}

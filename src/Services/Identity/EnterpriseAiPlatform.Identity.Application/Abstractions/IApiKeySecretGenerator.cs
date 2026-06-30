namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public sealed record GeneratedApiKeySecret(string ApiKey, string KeyPrefix, string Secret, string LastFour);

public interface IApiKeySecretGenerator
{
    GeneratedApiKeySecret Generate(Guid apiKeyId);
}

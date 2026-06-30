using EnterpriseAiPlatform.Identity.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Security;

public sealed class ApiKeySecretGenerator : IApiKeySecretGenerator
{
    private readonly ApiKeySecurityOptions _options;

    public ApiKeySecretGenerator(IOptions<ApiKeySecurityOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    public GeneratedApiKeySecret Generate(Guid apiKeyId)
    {
        if (apiKeyId == Guid.Empty)
        {
            throw new ArgumentException("API key identifier is required.", nameof(apiKeyId));
        }

        string secret = Base64UrlEncoding.EncodeRandomBytes(_options.SecretBytes);
        string keyPrefix = $"eap_{NormalizeEnvironmentName(_options.EnvironmentName)}_{apiKeyId:N}";
        string apiKey = $"{keyPrefix}.{secret}";
        string lastFour = secret[^4..];

        return new GeneratedApiKeySecret(apiKey, keyPrefix, secret, lastFour);
    }

    private static string NormalizeEnvironmentName(string environmentName)
    {
        if (string.IsNullOrWhiteSpace(environmentName))
        {
            return "prod";
        }

        string normalized = new(
            environmentName
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());

        return normalized.Length == 0 ? "prod" : normalized;
    }
}

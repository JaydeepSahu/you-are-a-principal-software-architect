using System.Security.Cryptography;
using System.Text;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Security;

public sealed class ApiKeyHasher : IApiKeyHasher
{
    private readonly ApiKeySecurityOptions _options;

    public ApiKeyHasher(IOptions<ApiKeySecurityOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    public string Hash(Guid apiKeyId, string secret)
    {
        if (apiKeyId == Guid.Empty)
        {
            throw new ArgumentException("API key identifier is required.", nameof(apiKeyId));
        }

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException("API key secret is required.", nameof(secret));
        }

        using HMACSHA256 hmac = CreateHmac();
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{apiKeyId:N}.{secret}"));
        return Convert.ToBase64String(hash);
    }

    public bool Verify(Guid apiKeyId, string secret, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        byte[] actualHashBytes = Convert.FromBase64String(Hash(apiKeyId, secret));
        byte[] expectedHashBytes = Convert.FromBase64String(expectedHash);
        return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
    }

    private HMACSHA256 CreateHmac()
    {
        if (string.IsNullOrWhiteSpace(_options.Pepper))
        {
            throw new InvalidOperationException("API key pepper must be configured.");
        }

        return new HMACSHA256(Encoding.UTF8.GetBytes(_options.Pepper));
    }
}

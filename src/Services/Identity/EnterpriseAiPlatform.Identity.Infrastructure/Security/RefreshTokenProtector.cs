using System.Security.Cryptography;
using System.Text;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Security;

public sealed class RefreshTokenProtector : IRefreshTokenProtector
{
    private readonly RefreshTokenSecurityOptions _options;

    public RefreshTokenProtector(IOptions<RefreshTokenSecurityOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    public string Generate()
    {
        return Base64UrlEncoding.EncodeRandomBytes(_options.TokenBytes);
    }

    public string Hash(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.", nameof(refreshToken));
        }

        using HMACSHA256 hmac = CreateHmac();
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(refreshToken)));
    }

    public bool Verify(string refreshToken, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        byte[] actualHashBytes = Convert.FromBase64String(Hash(refreshToken));
        byte[] expectedHashBytes = Convert.FromBase64String(expectedHash);
        return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
    }

    private HMACSHA256 CreateHmac()
    {
        if (string.IsNullOrWhiteSpace(_options.Pepper))
        {
            throw new InvalidOperationException("Refresh token pepper must be configured.");
        }

        return new HMACSHA256(Encoding.UTF8.GetBytes(_options.Pepper));
    }
}

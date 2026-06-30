using System.Security.Cryptography;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Security;

internal static class Base64UrlEncoding
{
    public static string EncodeRandomBytes(int byteCount)
    {
        if (byteCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount), "Byte count must be positive.");
        }

        return Encode(RandomNumberGenerator.GetBytes(byteCount));
    }

    public static string Encode(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal);
    }
}

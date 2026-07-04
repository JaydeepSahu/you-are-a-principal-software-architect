using System.Security.Cryptography;
using System.Text;

namespace EnterpriseAiPlatform.Knowledge.Domain;

public sealed class KnowledgeDocumentVersion : SharedKernel.ValueObject
{
    public KnowledgeDocumentVersion(int versionNumber, string contentHash, DateTimeOffset indexedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(versionNumber, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);

        VersionNumber = versionNumber;
        ContentHash = contentHash;
        IndexedAtUtc = indexedAtUtc;
    }

    public int VersionNumber { get; }

    public string ContentHash { get; }

    public DateTimeOffset IndexedAtUtc { get; }

    public static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return VersionNumber;
        yield return ContentHash;
        yield return IndexedAtUtc;
    }
}

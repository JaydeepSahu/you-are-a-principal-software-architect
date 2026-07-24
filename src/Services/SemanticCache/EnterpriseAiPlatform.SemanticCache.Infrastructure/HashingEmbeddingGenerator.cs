using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using EnterpriseAiPlatform.SemanticCache.Application.Cache;

namespace EnterpriseAiPlatform.SemanticCache.Infrastructure;

public sealed class HashingEmbeddingGenerator : IEmbeddingGenerator
{
    public const int Dimensions = 256;

    public double[] Generate(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        var vector = new double[Dimensions];

        foreach (Match match in Regex.Matches(text.ToLowerInvariant(), @"[\p{L}\p{N}_]+"))
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(match.Value));
            var index = BitConverter.ToUInt16(bytes, 0) % Dimensions;
            vector[index] += (bytes[2] & 1) == 0 ? 1 : -1;
        }

        Normalize(vector);
        return vector;
    }

    private static void Normalize(double[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(v => v * v));
        if (magnitude == 0) return;
        for (var i = 0; i < vector.Length; i++) vector[i] /= magnitude;
    }
}

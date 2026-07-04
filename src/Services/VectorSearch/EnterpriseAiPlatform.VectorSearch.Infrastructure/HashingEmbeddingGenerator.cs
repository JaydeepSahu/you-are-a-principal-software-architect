using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using EnterpriseAiPlatform.VectorSearch.Application.Abstractions;

namespace EnterpriseAiPlatform.VectorSearch.Infrastructure;

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

    public static double Cosine(double[] left, double[] right)
    {
        var length = Math.Min(left.Length, right.Length);
        var score = 0d;
        for (var i = 0; i < length; i++)
        {
            score += left[i] * right[i];
        }

        return Math.Max(0, score);
    }

    private static void Normalize(double[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(value => value * value));
        if (magnitude == 0)
        {
            return;
        }

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] /= magnitude;
        }
    }
}

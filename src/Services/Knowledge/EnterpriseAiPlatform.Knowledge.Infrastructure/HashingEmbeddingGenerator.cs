using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;

namespace EnterpriseAiPlatform.Knowledge.Infrastructure;

public sealed class HashingEmbeddingGenerator : IEmbeddingGenerator
{
    public const int Dimensions = 128;

    public double[] GenerateEmbedding(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var vector = new double[Dimensions];
        foreach (Match match in Regex.Matches(text.ToLowerInvariant(), @"[\p{L}\p{N}_]+"))
        {
            var token = match.Value;
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            var index = BitConverter.ToUInt16(hash, 0) % Dimensions;
            var sign = (hash[2] & 1) == 0 ? 1d : -1d;
            vector[index] += sign;
        }

        Normalize(vector);
        return vector;
    }

    internal static double CosineSimilarity(double[] left, double[] right)
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

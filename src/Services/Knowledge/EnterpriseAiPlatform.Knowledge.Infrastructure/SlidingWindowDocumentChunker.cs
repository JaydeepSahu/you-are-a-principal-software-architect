using System.Text.RegularExpressions;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Models;
using EnterpriseAiPlatform.Knowledge.Domain;

namespace EnterpriseAiPlatform.Knowledge.Infrastructure;

public sealed class SlidingWindowDocumentChunker : IDocumentChunker
{
    public IReadOnlyList<KnowledgeChunk> Chunk(
        KnowledgeDocumentId documentId,
        string content,
        ChunkingOptions options,
        IReadOnlyDictionary<string, string> metadata)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var tokens = Regex.Matches(content, @"\S+")
            .Select(match => match.Value)
            .ToList();
        if (tokens.Count == 0)
        {
            return [];
        }

        var chunks = new List<KnowledgeChunk>();
        var stride = options.MaxTokens - options.OverlapTokens;
        var ordinal = 0;

        for (var start = 0; start < tokens.Count; start += stride)
        {
            var window = tokens.Skip(start).Take(options.MaxTokens).ToList();
            if (window.Count == 0)
            {
                break;
            }

            var chunkMetadata = new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase)
            {
                ["chunk_start_token"] = start.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["chunk_end_token"] = (start + window.Count - 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
            };

            chunks.Add(new KnowledgeChunk(
                KnowledgeChunkId.New(),
                documentId,
                ordinal++,
                string.Join(' ', window),
                window.Count,
                chunkMetadata));

            if (start + window.Count >= tokens.Count)
            {
                break;
            }
        }

        return chunks;
    }
}

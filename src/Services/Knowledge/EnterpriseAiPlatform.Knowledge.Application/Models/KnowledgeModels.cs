using EnterpriseAiPlatform.Knowledge.Domain;

namespace EnterpriseAiPlatform.Knowledge.Application.Models;

public sealed record KnowledgeIngestionDocument(
    KnowledgeSource Source,
    string Title,
    string Content,
    string? ContentType,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record ChunkingOptions
{
    public ChunkingOptions(int maxTokens = 800, int overlapTokens = 80)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxTokens, 64);
        ArgumentOutOfRangeException.ThrowIfNegative(overlapTokens);
        if (overlapTokens >= maxTokens)
        {
            throw new ArgumentException("OverlapTokens must be lower than MaxTokens.", nameof(overlapTokens));
        }

        MaxTokens = maxTokens;
        OverlapTokens = overlapTokens;
    }

    public int MaxTokens { get; }

    public int OverlapTokens { get; }
}

public sealed record EmbeddedKnowledgeChunk(KnowledgeChunk Chunk, double[] Embedding);

public sealed record IndexedDocumentResult(KnowledgeDocument Document, bool Created, bool Changed);

public sealed record KnowledgeSearchHit(KnowledgeDocument Document, KnowledgeChunk Chunk, double Score);

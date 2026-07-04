using System.Collections.Concurrent;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Models;
using EnterpriseAiPlatform.Knowledge.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Infrastructure;

public sealed class InMemoryKnowledgeIndex(IEmbeddingGenerator embeddingGenerator) : IKnowledgeIndex
{
    private readonly ConcurrentDictionary<Guid, IndexedChunk> _chunks = new();

    public Task IndexAsync(
        KnowledgeDocument document,
        IReadOnlyList<EmbeddedKnowledgeChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        foreach (var chunk in chunks)
        {
            _chunks[chunk.Chunk.Id.Value] = new IndexedChunk(document, chunk.Chunk, chunk.Embedding);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(
        KnowledgeDocumentId documentId,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in _chunks.Where(entry =>
            entry.Value.Document.Id == documentId &&
            entry.Value.Document.TenantId == tenantId))
        {
            _chunks.TryRemove(entry.Key, out _);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<KnowledgeSearchHit>> SearchAsync(
        TenantId tenantId,
        string query,
        IReadOnlySet<KnowledgeSourceType>? sourceTypes,
        int take,
        double minScore,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = embeddingGenerator.GenerateEmbedding(query);
        IReadOnlyList<KnowledgeSearchHit> hits = _chunks.Values
            .Where(entry => entry.Document.TenantId == tenantId)
            .Where(entry => sourceTypes is null || sourceTypes.Contains(entry.Document.Source.Type))
            .Select(entry => new KnowledgeSearchHit(
                entry.Document,
                entry.Chunk,
                HashingEmbeddingGenerator.CosineSimilarity(queryEmbedding, entry.Embedding)))
            .Where(hit => hit.Score >= minScore)
            .OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.Document.Metadata.Title)
            .Take(take)
            .ToList();

        return Task.FromResult(hits);
    }

    private sealed record IndexedChunk(KnowledgeDocument Document, KnowledgeChunk Chunk, double[] Embedding);
}

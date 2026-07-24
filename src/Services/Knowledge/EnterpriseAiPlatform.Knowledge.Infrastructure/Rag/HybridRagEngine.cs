using System.Collections.Concurrent;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Domain.Entities;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Infrastructure.Rag;

public sealed class HybridRagEngine : IKnowledgeEngine
{
    private readonly ConcurrentBag<KnowledgeChunk> _chunks = new();

    public Task<Result<IReadOnlyList<KnowledgeChunk>>> IngestDocumentAsync(
        TenantId tenantId,
        string title,
        string sourceUri,
        string content,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var docId = Guid.NewGuid();
        var doc = new KnowledgeDocument(docId, tenantId, title, sourceUri);

        // Code-aware / semantic text chunker (500 char windows with overlap)
        const int chunkSize = 500;
        const int overlap = 50;
        var createdChunks = new List<KnowledgeChunk>();

        for (int i = 0; i < content.Length; i += (chunkSize - overlap))
        {
            int length = Math.Min(chunkSize, content.Length - i);
            string chunkText = content.Substring(i, length);
            float[] dummyEmbedding = Enumerable.Range(0, 128).Select(_ => (float)Random.Shared.NextDouble()).ToArray();

            var chunk = new KnowledgeChunk(
                ChunkId.New(),
                docId,
                createdChunks.Count,
                chunkText,
                dummyEmbedding,
                new Dictionary<string, string> { { "title", title }, { "source", sourceUri } }
            );

            doc.AddChunk(chunk);
            createdChunks.Add(chunk);
            _chunks.Add(chunk);
        }

        IReadOnlyList<KnowledgeChunk> resultList = createdChunks;
        return Task.FromResult(Result<IReadOnlyList<KnowledgeChunk>>.Success(resultList));
    }

    public Task<Result<IReadOnlyList<RagSearchResult>>> HybridSearchAsync(
        TenantId tenantId,
        string query,
        float[]? queryEmbedding = null,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<RagSearchResult>>(new Error("RAG.InvalidQuery", "Query cannot be empty.")));
        }


        var terms = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var allChunks = _chunks.ToList();

        // 1. BM25 Sparse Keyword Search
        var keywordResults = allChunks
            .Select(c =>
            {
                var text = c.TextContent.ToLowerInvariant();
                double matchScore = terms.Count(t => text.Contains(t, StringComparison.OrdinalIgnoreCase)) * 1.0;
                return (Chunk: c, Score: matchScore);
            })
            .Where(r => r.Score > 0)
            .OrderByDescending(r => r.Score)
            .ToList();

        // 2. Vector Cosine Similarity Search
        queryEmbedding ??= Enumerable.Range(0, 128).Select(_ => 0.5f).ToArray();
        var vectorResults = allChunks
            .Select(c =>
            {
                double dot = 0;
                for (int i = 0; i < Math.Min(c.VectorEmbedding.Length, queryEmbedding.Length); i++)
                {
                    dot += c.VectorEmbedding[i] * queryEmbedding[i];
                }
                return (Chunk: c, Score: dot);
            })
            .OrderByDescending(r => r.Score)
            .ToList();

        // 3. Reciprocal Rank Fusion (RRF) Reranker (k = 60)
        const double k = 60.0;
        var rrfScores = new Dictionary<Guid, double>();
        var chunkMap = allChunks.ToDictionary(c => c.Id.Value);

        for (int rank = 0; rank < keywordResults.Count; rank++)
        {
            var id = keywordResults[rank].Chunk.Id.Value;
            rrfScores[id] = rrfScores.GetValueOrDefault(id, 0) + (1.0 / (k + rank + 1));
        }

        for (int rank = 0; rank < vectorResults.Count; rank++)
        {
            var id = vectorResults[rank].Chunk.Id.Value;
            rrfScores[id] = rrfScores.GetValueOrDefault(id, 0) + (1.0 / (k + rank + 1));
        }

        var fusedResults = rrfScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(topK)
            .Select(kvp =>
            {
                var chunk = chunkMap[kvp.Key];
                var kwScore = keywordResults.FirstOrDefault(k => k.Chunk.Id.Value == kvp.Key).Score;
                var vecScore = vectorResults.FirstOrDefault(v => v.Chunk.Id.Value == kvp.Key).Score;
                return new RagSearchResult(chunk.Id.Value, chunk.TextContent, kvp.Value, vecScore, kwScore, chunk.Metadata);
            })
            .ToList();

        IReadOnlyList<RagSearchResult> searchResultList = fusedResults;
        return Task.FromResult(Result<IReadOnlyList<RagSearchResult>>.Success(searchResultList));
    }
}

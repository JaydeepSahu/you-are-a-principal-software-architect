using System.Collections.Concurrent;
using EnterpriseAiPlatform.SharedKernel;
using EnterpriseAiPlatform.VectorSearch.Application.Abstractions;
using EnterpriseAiPlatform.VectorSearch.Domain;

namespace EnterpriseAiPlatform.VectorSearch.Infrastructure;

public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<Guid, VectorRecord> _records = new();

    public VectorSearchProvider Provider => VectorSearchProvider.InMemory;

    public Task UpsertAsync(IReadOnlyList<VectorRecord> records, CancellationToken cancellationToken = default)
    {
        foreach (var record in records)
        {
            _records[record.Id.Value] = record;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VectorSearchCandidate>> SearchAsync(
        TenantId tenantId,
        double[] queryEmbedding,
        IReadOnlyDictionary<string, string> metadataFilters,
        int candidateCount,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VectorSearchCandidate> candidates = _records.Values
            .Where(record => record.TenantId == tenantId)
            .Where(record => MatchesFilters(record, metadataFilters))
            .Select(record => new VectorSearchCandidate(record, HashingEmbeddingGenerator.Cosine(queryEmbedding, record.Embedding)))
            .OrderByDescending(candidate => candidate.SemanticScore)
            .Take(candidateCount)
            .ToList();

        return Task.FromResult(candidates);
    }

    public Task<int> CountAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult(_records.Values.Count(record => record.TenantId == tenantId));

    private static bool MatchesFilters(VectorRecord record, IReadOnlyDictionary<string, string> filters)
    {
        foreach (var (key, value) in filters)
        {
            if (!record.Metadata.TryGetValue(key, out var actual) ||
                !actual.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}

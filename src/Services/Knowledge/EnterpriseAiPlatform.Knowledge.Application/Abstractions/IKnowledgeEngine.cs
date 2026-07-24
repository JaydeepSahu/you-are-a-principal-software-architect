using EnterpriseAiPlatform.Knowledge.Domain.Entities;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Application.Abstractions;

public sealed record RagSearchResult(
    Guid ChunkId,
    string TextContent,
    double CombinedScore,
    double VectorScore,
    double KeywordScore,
    IReadOnlyDictionary<string, string> Metadata);

public interface IKnowledgeEngine
{
    Task<Result<IReadOnlyList<KnowledgeChunk>>> IngestDocumentAsync(
        TenantId tenantId,
        string title,
        string sourceUri,
        string content,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<RagSearchResult>>> HybridSearchAsync(
        TenantId tenantId,
        string query,
        float[]? queryEmbedding = null,
        int topK = 5,
        CancellationToken cancellationToken = default);
}

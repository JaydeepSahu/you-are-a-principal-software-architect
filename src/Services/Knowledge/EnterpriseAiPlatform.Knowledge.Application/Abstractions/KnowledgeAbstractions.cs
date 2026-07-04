using EnterpriseAiPlatform.Knowledge.Application.Models;
using EnterpriseAiPlatform.Knowledge.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Application.Abstractions;

public interface IKnowledgeRepository
{
    Task<KnowledgeDocument?> GetByIdAsync(KnowledgeDocumentId id, TenantId tenantId, CancellationToken cancellationToken = default);

    Task<KnowledgeDocument?> GetBySourceAsync(KnowledgeSource source, TenantId tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeDocument>> GetByTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    Task UpsertAsync(KnowledgeDocument document, CancellationToken cancellationToken = default);
}

public interface IKnowledgeSourceConnector
{
    KnowledgeSourceType SourceType { get; }

    Task<KnowledgeIngestionDocument> IngestAsync(KnowledgeIngestionDocumentRequest request, CancellationToken cancellationToken = default);
}

public sealed record KnowledgeIngestionDocumentRequest(
    KnowledgeSourceType SourceType,
    string ExternalId,
    string Title,
    string? Content,
    Uri? Uri,
    string? ContentType,
    IReadOnlyDictionary<string, string> Metadata);

public interface IDocumentChunker
{
    IReadOnlyList<KnowledgeChunk> Chunk(KnowledgeDocumentId documentId, string content, ChunkingOptions options, IReadOnlyDictionary<string, string> metadata);
}

public interface IEmbeddingGenerator
{
    double[] GenerateEmbedding(string text);
}

public interface IMetadataExtractor
{
    KnowledgeDocumentMetadata Extract(KnowledgeIngestionDocument document);
}

public interface IKnowledgeIndex
{
    Task IndexAsync(KnowledgeDocument document, IReadOnlyList<EmbeddedKnowledgeChunk> chunks, CancellationToken cancellationToken = default);

    Task RemoveAsync(KnowledgeDocumentId documentId, TenantId tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeSearchHit>> SearchAsync(TenantId tenantId, string query, IReadOnlySet<KnowledgeSourceType>? sourceTypes, int take, double minScore, CancellationToken cancellationToken = default);
}

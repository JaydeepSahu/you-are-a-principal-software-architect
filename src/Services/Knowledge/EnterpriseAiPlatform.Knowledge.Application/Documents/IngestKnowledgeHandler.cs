using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Models;
using EnterpriseAiPlatform.Knowledge.Contracts.Requests;
using EnterpriseAiPlatform.Knowledge.Contracts.Responses;
using EnterpriseAiPlatform.Knowledge.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Application.Documents;

public sealed class IngestKnowledgeHandler(
    IEnumerable<IKnowledgeSourceConnector> connectors,
    IKnowledgeRepository repository,
    IDocumentChunker chunker,
    IEmbeddingGenerator embeddingGenerator,
    IMetadataExtractor metadataExtractor,
    IKnowledgeIndex index,
    IRequestContextAccessor requestContext)
    : ICommandHandler<IngestKnowledgeCommand, IngestKnowledgeResponse>
{
    public async Task<Result<IngestKnowledgeResponse>> Handle(
        IngestKnowledgeCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = requestContext.Current.TenantId;
        var indexedAtUtc = DateTimeOffset.UtcNow;
        var connectorMap = connectors.ToDictionary(connector => connector.SourceType);
        var chunking = new ChunkingOptions(
            command.Request.Chunking?.MaxTokens ?? 800,
            command.Request.Chunking?.OverlapTokens ?? 80);
        var results = new List<IngestedDocumentResponse>();
        var created = 0;
        var updated = 0;
        var skipped = 0;

        foreach (var item in command.Request.Items)
        {
            if (!TryParseSourceType(item.SourceType, out var sourceType))
            {
                return Result.Failure<IngestKnowledgeResponse>(KnowledgeErrors.ValidationFailed($"Unsupported source type '{item.SourceType}'."));
            }

            if (!connectorMap.TryGetValue(sourceType, out var connector))
            {
                return Result.Failure<IngestKnowledgeResponse>(KnowledgeErrors.ConnectorNotFound(item.SourceType));
            }

            var ingestionDocument = await connector.IngestAsync(
                new KnowledgeIngestionDocumentRequest(
                    sourceType,
                    item.ExternalId,
                    item.Title,
                    item.Content,
                    item.Uri,
                    item.ContentType,
                    item.Metadata ?? new Dictionary<string, string>()),
                cancellationToken);
            var metadata = metadataExtractor.Extract(ingestionDocument);
            var contentHash = KnowledgeDocumentVersion.ComputeHash(ingestionDocument.Content);
            var existing = await repository.GetBySourceAsync(ingestionDocument.Source, tenantId, cancellationToken);

            if (existing is not null && existing.CurrentContentHash == contentHash && !command.Request.ForceReindex)
            {
                skipped++;
                results.Add(Map(existing, false, indexedAtUtc));
                continue;
            }

            var document = existing ?? new KnowledgeDocument(
                KnowledgeDocumentId.New(),
                tenantId,
                ingestionDocument.Source,
                metadata,
                contentHash,
                indexedAtUtc);

            var chunks = chunker.Chunk(document.Id, ingestionDocument.Content, chunking, metadata.Attributes);
            var embeddedChunks = chunks
                .Select(chunk => new EmbeddedKnowledgeChunk(chunk, embeddingGenerator.GenerateEmbedding(chunk.Text)))
                .ToList();

            var changed = document.ApplyIndex(
                ingestionDocument.Source,
                metadata,
                contentHash,
                chunks,
                indexedAtUtc);

            await repository.UpsertAsync(document, cancellationToken);
            await index.RemoveAsync(document.Id, tenantId, cancellationToken);
            await index.IndexAsync(document, embeddedChunks, cancellationToken);

            if (existing is null)
            {
                created++;
            }
            else if (changed || command.Request.ForceReindex)
            {
                updated++;
            }

            results.Add(Map(document, existing is null || changed || command.Request.ForceReindex, indexedAtUtc));
        }

        return Result.Success(new IngestKnowledgeResponse(results, created, updated, skipped));
    }

    private static bool TryParseSourceType(string sourceType, out KnowledgeSourceType parsed)
    {
        return Enum.TryParse(sourceType, ignoreCase: true, out parsed);
    }

    private static IngestedDocumentResponse Map(KnowledgeDocument document, bool changed, DateTimeOffset indexedAtUtc)
        => new(
            document.Id.Value,
            document.Source.Type.ToString(),
            document.Source.ExternalId,
            document.Metadata.Title,
            document.CurrentVersion,
            changed,
            document.Chunks.Count,
            indexedAtUtc);
}

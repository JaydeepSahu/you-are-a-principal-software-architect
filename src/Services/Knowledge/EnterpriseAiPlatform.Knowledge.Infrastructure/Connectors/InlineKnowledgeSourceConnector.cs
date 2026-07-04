using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Models;
using EnterpriseAiPlatform.Knowledge.Domain;

namespace EnterpriseAiPlatform.Knowledge.Infrastructure.Connectors;

public sealed class InlineKnowledgeSourceConnector(KnowledgeSourceType sourceType) : IKnowledgeSourceConnector
{
    public KnowledgeSourceType SourceType { get; } = sourceType;

    public Task<KnowledgeIngestionDocument> IngestAsync(
        KnowledgeIngestionDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SourceType != SourceType)
        {
            throw new InvalidOperationException($"Connector '{SourceType}' cannot ingest source type '{request.SourceType}'.");
        }

        var content = request.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            content = request.Uri is null
                ? throw new ArgumentException("Either content or URI must be provided for knowledge ingestion.", nameof(request))
                : $"External {SourceType} source '{request.Title}' at {request.Uri}. Configure a credentialed {SourceType} connector to hydrate full remote content.";
        }

        var attributes = new Dictionary<string, string>(request.Metadata, StringComparer.OrdinalIgnoreCase)
        {
            ["source_type"] = SourceType.ToString(),
            ["external_id"] = request.ExternalId,
        };

        if (request.Uri is not null)
        {
            attributes["source_uri"] = request.Uri.ToString();
        }

        return Task.FromResult(new KnowledgeIngestionDocument(
            new KnowledgeSource(SourceType, request.ExternalId, request.Uri),
            request.Title,
            content,
            request.ContentType,
            attributes));
    }
}

using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Contracts.Responses;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Application.Documents;

internal sealed class GetKnowledgeDocumentHandler(
    IKnowledgeRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetKnowledgeDocumentQuery, KnowledgeDocumentResponse>
{
    public async Task<Result<KnowledgeDocumentResponse>> Handle(
        GetKnowledgeDocumentQuery query,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(query.DocumentId, requestContext.Current.TenantId, cancellationToken);
        return document is null
            ? Result.Failure<KnowledgeDocumentResponse>(KnowledgeErrors.DocumentNotFound())
            : Result.Success(DocumentMapper.Map(document));
    }
}

internal sealed class GetKnowledgeDocumentVersionsHandler(
    IKnowledgeRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetKnowledgeDocumentVersionsQuery, IReadOnlyList<KnowledgeDocumentVersionResponse>>
{
    public async Task<Result<IReadOnlyList<KnowledgeDocumentVersionResponse>>> Handle(
        GetKnowledgeDocumentVersionsQuery query,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(query.DocumentId, requestContext.Current.TenantId, cancellationToken);
        return document is null
            ? Result.Failure<IReadOnlyList<KnowledgeDocumentVersionResponse>>(KnowledgeErrors.DocumentNotFound())
            : Result.Success<IReadOnlyList<KnowledgeDocumentVersionResponse>>(
                document.Versions
                    .Select(version => new KnowledgeDocumentVersionResponse(
                        version.VersionNumber,
                        version.ContentHash,
                        version.IndexedAtUtc))
                    .ToList());
    }
}

internal static class DocumentMapper
{
    public static KnowledgeDocumentResponse Map(Domain.KnowledgeDocument document)
        => new(
            document.Id.Value,
            document.Source.Type.ToString(),
            document.Source.ExternalId,
            document.Source.Uri,
            document.Metadata.Title,
            document.Metadata.ContentType,
            document.Metadata.Language,
            document.CurrentVersion,
            document.CurrentContentHash,
            document.Chunks.Count,
            document.Metadata.Attributes,
            document.CreatedAtUtc,
            document.UpdatedAtUtc);
}

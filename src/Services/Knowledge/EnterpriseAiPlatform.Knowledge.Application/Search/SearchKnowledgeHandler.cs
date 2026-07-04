using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Contracts.Responses;
using EnterpriseAiPlatform.Knowledge.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Application.Search;

public sealed class SearchKnowledgeHandler(
    IKnowledgeIndex index,
    IRequestContextAccessor requestContext)
    : IQueryHandler<SearchKnowledgeQuery, SearchKnowledgeResponse>
{
    public async Task<Result<SearchKnowledgeResponse>> Handle(
        SearchKnowledgeQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlySet<KnowledgeSourceType>? sourceTypes = null;
        if (query.Request.SourceTypes is { Count: > 0 })
        {
            var parsed = new HashSet<KnowledgeSourceType>();
            foreach (var sourceType in query.Request.SourceTypes)
            {
                if (!Enum.TryParse(sourceType, ignoreCase: true, out KnowledgeSourceType value))
                {
                    return Result.Failure<SearchKnowledgeResponse>(KnowledgeErrors.ValidationFailed($"Unsupported source type '{sourceType}'."));
                }

                parsed.Add(value);
            }

            sourceTypes = parsed;
        }

        var hits = await index.SearchAsync(
            requestContext.Current.TenantId,
            query.Request.Query,
            sourceTypes,
            query.Request.Take,
            query.Request.MinScore,
            cancellationToken);

        return Result.Success(new SearchKnowledgeResponse(
            query.Request.Query,
            hits.Select(hit => new SearchHitResponse(
                    hit.Document.Id.Value,
                    hit.Chunk.Id.Value,
                    hit.Document.Source.Type.ToString(),
                    hit.Document.Source.ExternalId,
                    hit.Document.Metadata.Title,
                    hit.Document.CurrentVersion,
                    hit.Chunk.Ordinal,
                    hit.Chunk.Text,
                    Math.Round(hit.Score, 4),
                    hit.Chunk.Metadata))
                .ToList()));
    }
}

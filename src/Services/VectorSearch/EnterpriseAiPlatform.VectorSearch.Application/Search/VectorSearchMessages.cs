using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.VectorSearch.Contracts.Requests;
using EnterpriseAiPlatform.VectorSearch.Contracts.Responses;

namespace EnterpriseAiPlatform.VectorSearch.Application.Search;

public sealed record UpsertVectorDocumentsCommand(UpsertVectorDocumentsRequest Request) : ICommand<UpsertVectorDocumentsResponse>;

public sealed record SearchVectorsQuery(SearchVectorsRequest Request) : IQuery<SearchVectorsResponse>;

public sealed record GetVectorSearchProgressQuery : IQuery<VectorSearchProgressResponse>;

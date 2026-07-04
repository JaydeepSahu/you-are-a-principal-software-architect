using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Contracts.Requests;
using EnterpriseAiPlatform.Knowledge.Contracts.Responses;

namespace EnterpriseAiPlatform.Knowledge.Application.Search;

public sealed record SearchKnowledgeQuery(SearchKnowledgeRequest Request) : IQuery<SearchKnowledgeResponse>;

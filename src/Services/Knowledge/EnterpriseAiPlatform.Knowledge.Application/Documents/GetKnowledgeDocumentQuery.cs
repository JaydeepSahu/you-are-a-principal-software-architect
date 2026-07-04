using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Contracts.Responses;
using EnterpriseAiPlatform.Knowledge.Domain;

namespace EnterpriseAiPlatform.Knowledge.Application.Documents;

public sealed record GetKnowledgeDocumentQuery(KnowledgeDocumentId DocumentId) : IQuery<KnowledgeDocumentResponse>;

public sealed record GetKnowledgeDocumentVersionsQuery(KnowledgeDocumentId DocumentId) : IQuery<IReadOnlyList<KnowledgeDocumentVersionResponse>>;

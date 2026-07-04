using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Contracts.Requests;
using EnterpriseAiPlatform.Knowledge.Contracts.Responses;

namespace EnterpriseAiPlatform.Knowledge.Application.Documents;

public sealed record IngestKnowledgeCommand(IngestKnowledgeRequest Request) : ICommand<IngestKnowledgeResponse>;

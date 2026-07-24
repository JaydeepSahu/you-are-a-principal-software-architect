using EnterpriseAiPlatform.Agents.Contracts.Requests;
using EnterpriseAiPlatform.Agents.Contracts.Responses;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.Agents.Application.Commands;

public sealed record ExecuteAgentPlanCommand(
    TenantId TenantId,
    AgentExecutionRequest Request) : IRequest<Result<AgentExecutionResponse>>;

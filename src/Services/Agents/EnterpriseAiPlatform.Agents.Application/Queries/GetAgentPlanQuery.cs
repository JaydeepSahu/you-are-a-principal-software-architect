using EnterpriseAiPlatform.Agents.Contracts.Responses;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.Agents.Application.Queries;

public sealed record GetAgentPlanQuery(TenantId TenantId, Guid PlanId) : IRequest<Result<AgentExecutionResponse>>;

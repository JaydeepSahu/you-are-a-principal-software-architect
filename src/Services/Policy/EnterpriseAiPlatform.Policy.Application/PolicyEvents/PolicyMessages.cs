using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Policy.Contracts.Requests;
using EnterpriseAiPlatform.Policy.Contracts.Responses;

namespace EnterpriseAiPlatform.Policy.Application.PolicyEvents;

public sealed record CreatePolicyCommand(CreatePolicyRequest Request) : ICommand<PolicyCreatedResponse>;
public sealed record EvaluatePolicyCommand(EvaluatePolicyRequest Request) : IQuery<PolicyEvaluatedResponse>;
public sealed record ListPoliciesQuery : IQuery<PolicyListResponse>;

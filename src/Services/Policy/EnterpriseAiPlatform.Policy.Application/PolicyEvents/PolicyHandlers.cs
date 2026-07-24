using PolicyEntity = EnterpriseAiPlatform.Policy.Domain.Policy;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Policy.Application.Abstractions;
using EnterpriseAiPlatform.Policy.Application.PolicyEvents;
using EnterpriseAiPlatform.Policy.Contracts.Requests;
using EnterpriseAiPlatform.Policy.Contracts.Responses;
using EnterpriseAiPlatform.Policy.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Policy.Application.PolicyEvents;

public sealed class CreatePolicyHandler(
    IPolicyRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<CreatePolicyCommand, PolicyCreatedResponse>
{
    public async Task<Result<PolicyCreatedResponse>> Handle(
        CreatePolicyCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var resourceType = Enum.TryParse<PolicyResourceType>(command.Request.ResourceType, true, out var rt) ? rt : PolicyResourceType.AiGateway;
        var effect = Enum.TryParse<PolicyEffect>(command.Request.Effect, true, out var ef) ? ef : PolicyEffect.Allow;

        var policy = new PolicyEntity(
            PolicyId.New(),
            requestContext.Current.TenantId,
            command.Request.Name,
            command.Request.Description,
            resourceType,
            effect,
            command.Request.Principals,
            command.Request.Actions,
            command.Request.Conditions,
            isEnabled: true,
            now,
            command.Request.ExpiresAtUtc);

        await repository.AddAsync(policy, cancellationToken);
        return Result.Success(new PolicyCreatedResponse(policy.Id.Value, now));
    }
}

public sealed class EvaluatePolicyHandler(
    IPolicyRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<EvaluatePolicyCommand, PolicyEvaluatedResponse>
{
    public async Task<Result<PolicyEvaluatedResponse>> Handle(
        EvaluatePolicyCommand command,
        CancellationToken cancellationToken)
    {
        PolicyResourceType? resourceType = Enum.TryParse<PolicyResourceType>(command.Request.ResourceType, true, out var rt) ? rt : null;

        var policies = await repository.QueryAsync(
            requestContext.Current.TenantId,
            resourceType,
            cancellationToken);

        var applicablePolicies = policies
            .Where(p => p.IsEnabled && !p.IsExpired)
            .Where(p => p.Actions.Any(a => a.Equals(command.Request.Action, StringComparison.OrdinalIgnoreCase) || a == "*"))
            .ToList();

        var matched = applicablePolicies
            .OrderByDescending(p => p.Effect)
            .FirstOrDefault();

        if (matched is null)
        {
            return Result.Success(new PolicyEvaluatedResponse(true, null, "Allow", "Default allow — no matching policy."));
        }

        return Result.Success(new PolicyEvaluatedResponse(
            matched.Effect == PolicyEffect.Allow,
            matched.Id.Value,
            matched.Effect.ToString(),
            matched.Name));
    }
}

public sealed class ListPoliciesHandler(
    IPolicyRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<ListPoliciesQuery, PolicyListResponse>
{
    public async Task<Result<PolicyListResponse>> Handle(
        ListPoliciesQuery query,
        CancellationToken cancellationToken)
    {
        var policies = await repository.QueryAsync(
            requestContext.Current.TenantId,
            null,
            cancellationToken);

        return Result.Success(new PolicyListResponse(
            policies.Count,
            policies.Select(p => new PolicySummaryResponse(
                p.Id.Value,
                p.Name,
                p.ResourceType.ToString(),
                p.Effect.ToString(),
                p.IsEnabled,
                p.CreatedAtUtc,
                p.ExpiresAtUtc)).ToList()));
    }
}

using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationCommands;

internal sealed class AcknowledgeAlertHandler : IRequestHandler<AcknowledgeAlertCommand, Result<AlertCommandResult>>
{
    private readonly IAlertRepository _alertRepository;
    private readonly IRequestContext _requestContext;

    public AcknowledgeAlertHandler(IAlertRepository alertRepository, IRequestContext requestContext)
    {
        _alertRepository = alertRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<AlertCommandResult>> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetByIdAsync(AlertId.Create(Guid.Parse(request.Request.AlertId)), _requestContext.TenantId, cancellationToken);
        if (alert is null)
            return Result.Failure<AlertCommandResult>(CostOptimizationErrors.AlertNotFound);

        alert.Acknowledge(request.Request.AcknowledgedBy);
        await _alertRepository.UpdateAsync(alert, cancellationToken);

        return new AlertCommandResult(
            alert.Id.Value.ToString(),
            alert.Type,
            alert.Title,
            alert.Status.ToString());
    }
}

internal sealed class ResolveAlertHandler : IRequestHandler<ResolveAlertCommand, Result<AlertCommandResult>>
{
    private readonly IAlertRepository _alertRepository;
    private readonly IRequestContext _requestContext;

    public ResolveAlertHandler(IAlertRepository alertRepository, IRequestContext requestContext)
    {
        _alertRepository = alertRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<AlertCommandResult>> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetByIdAsync(AlertId.Create(Guid.Parse(request.Request.AlertId)), _requestContext.TenantId, cancellationToken);
        if (alert is null)
            return Result.Failure<AlertCommandResult>(CostOptimizationErrors.AlertNotFound);

        alert.Resolve(request.Request.ResolvedBy, request.Request.Notes);
        await _alertRepository.UpdateAsync(alert, cancellationToken);

        return new AlertCommandResult(
            alert.Id.Value.ToString(),
            alert.Type,
            alert.Title,
            alert.Status.ToString());
    }
}

internal sealed class CreateRoutingRuleHandler : IRequestHandler<CreateRoutingRuleCommand, Result<RoutingRuleCommandResult>>
{
    private readonly IRoutingRuleRepository _routingRuleRepository;
    private readonly IRequestContext _requestContext;

    public CreateRoutingRuleHandler(IRoutingRuleRepository routingRuleRepository, IRequestContext requestContext)
    {
        _routingRuleRepository = routingRuleRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<RoutingRuleCommandResult>> Handle(CreateRoutingRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = RoutingRule.Create(
            _requestContext.TenantId,
            request.Request.Name,
            request.Request.Description,
            request.Request.TriggerType,
            request.Request.SourceModelId,
            request.Request.TargetModelId,
            request.Request.CostThreshold,
            request.Request.UsageThresholdPercent,
            request.Request.IsAutomatic,
            request.Request.Priority,
            request.Request.ExpiresAt,
            request.Request.Conditions);

        await _routingRuleRepository.AddAsync(rule, cancellationToken);

        return new RoutingRuleCommandResult(
            rule.Id.Value.ToString(),
            rule.Name,
            rule.SourceModelId,
            rule.TargetModelId,
            rule.IsEnabled);
    }
}

internal sealed class UpdateRoutingRuleHandler : IRequestHandler<UpdateRoutingRuleCommand, Result<RoutingRuleCommandResult>>
{
    private readonly IRoutingRuleRepository _routingRuleRepository;
    private readonly IRequestContext _requestContext;

    public UpdateRoutingRuleHandler(IRoutingRuleRepository routingRuleRepository, IRequestContext requestContext)
    {
        _routingRuleRepository = routingRuleRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<RoutingRuleCommandResult>> Handle(UpdateRoutingRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _routingRuleRepository.GetByIdAsync(RoutingRuleId.Create(request.RuleId), _requestContext.TenantId, cancellationToken);
        if (rule is null)
            return Result.Failure<RoutingRuleCommandResult>(CostOptimizationErrors.RoutingRuleNotFound);

        if (request.Request.IsEnabled.HasValue)
        {
            if (request.Request.IsEnabled.Value)
                rule.Enable();
            else
                rule.Disable();
        }

        await _routingRuleRepository.UpdateAsync(rule, cancellationToken);

        return new RoutingRuleCommandResult(
            rule.Id.Value.ToString(),
            rule.Name,
            rule.SourceModelId,
            rule.TargetModelId,
            rule.IsEnabled);
    }
}

internal sealed class TriggerRoutingRuleHandler : IRequestHandler<TriggerRoutingRuleCommand, Result<RoutingRuleTriggerCommandResult>>
{
    private readonly IRoutingRuleRepository _routingRuleRepository;
    private readonly IRequestContext _requestContext;

    public TriggerRoutingRuleHandler(IRoutingRuleRepository routingRuleRepository, IRequestContext requestContext)
    {
        _routingRuleRepository = routingRuleRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<RoutingRuleTriggerCommandResult>> Handle(TriggerRoutingRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _routingRuleRepository.GetByIdAsync(RoutingRuleId.Create(Guid.Parse(request.Request.RuleId)), _requestContext.TenantId, cancellationToken);
        if (rule is null)
            return Result.Failure<RoutingRuleTriggerCommandResult>(CostOptimizationErrors.RoutingRuleNotFound);

        var shouldTrigger = rule.ShouldTrigger(request.Request.CurrentCost, request.Request.UsagePercent);

        if (shouldTrigger && rule.IsAutomatic)
        {
            rule.RecordTrigger();
            await _routingRuleRepository.UpdateAsync(rule, cancellationToken);
        }

        return new RoutingRuleTriggerCommandResult(
            rule.Id.Value.ToString(),
            rule.Name,
            rule.SourceModelId,
            rule.TargetModelId,
            shouldTrigger,
            shouldTrigger ? DateTimeOffset.UtcNow : null,
            shouldTrigger ? $"Rule '{rule.Name}' triggered: cost={request.Request.CurrentCost:C}, usage={request.Request.UsagePercent:F1}%" : null);
    }
}

internal sealed class DisableRoutingRuleHandler : IRequestHandler<DisableRoutingRuleCommand, Result<RoutingRuleCommandResult>>
{
    private readonly IRoutingRuleRepository _routingRuleRepository;
    private readonly IRequestContext _requestContext;

    public DisableRoutingRuleHandler(IRoutingRuleRepository routingRuleRepository, IRequestContext requestContext)
    {
        _routingRuleRepository = routingRuleRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<RoutingRuleCommandResult>> Handle(DisableRoutingRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _routingRuleRepository.GetByIdAsync(RoutingRuleId.Create(request.RuleId), _requestContext.TenantId, cancellationToken);
        if (rule is null)
            return Result.Failure<RoutingRuleCommandResult>(CostOptimizationErrors.RoutingRuleNotFound);

        rule.Disable();
        await _routingRuleRepository.UpdateAsync(rule, cancellationToken);

        return new RoutingRuleCommandResult(
            rule.Id.Value.ToString(),
            rule.Name,
            rule.SourceModelId,
            rule.TargetModelId,
            rule.IsEnabled);
    }
}

internal sealed class EnableRoutingRuleHandler : IRequestHandler<EnableRoutingRuleCommand, Result<RoutingRuleCommandResult>>
{
    private readonly IRoutingRuleRepository _routingRuleRepository;
    private readonly IRequestContext _requestContext;

    public EnableRoutingRuleHandler(IRoutingRuleRepository routingRuleRepository, IRequestContext requestContext)
    {
        _routingRuleRepository = routingRuleRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<RoutingRuleCommandResult>> Handle(EnableRoutingRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _routingRuleRepository.GetByIdAsync(RoutingRuleId.Create(request.RuleId), _requestContext.TenantId, cancellationToken);
        if (rule is null)
            return Result.Failure<RoutingRuleCommandResult>(CostOptimizationErrors.RoutingRuleNotFound);

        rule.Enable();
        await _routingRuleRepository.UpdateAsync(rule, cancellationToken);

        return new RoutingRuleCommandResult(
            rule.Id.Value.ToString(),
            rule.Name,
            rule.SourceModelId,
            rule.TargetModelId,
            rule.IsEnabled);
    }
}

using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationCommands;

internal sealed class RecordCostHandler : IRequestHandler<RecordCostCommand, Result<CostRecordCommandResult>>
{
    private readonly ICostRecordRepository _costRecordRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IDepartmentQuotaRepository _departmentQuotaRepository;
    private readonly IUserQuotaRepository _userQuotaRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IRequestContext _requestContext;

    public RecordCostHandler(
        ICostRecordRepository costRecordRepository,
        IBudgetRepository budgetRepository,
        IDepartmentQuotaRepository departmentQuotaRepository,
        IUserQuotaRepository userQuotaRepository,
        IAlertRepository alertRepository,
        IRequestContext requestContext)
    {
        _costRecordRepository = costRecordRepository;
        _budgetRepository = budgetRepository;
        _departmentQuotaRepository = departmentQuotaRepository;
        _userQuotaRepository = userQuotaRepository;
        _alertRepository = alertRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<CostRecordCommandResult>> Handle(RecordCostCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CostCategory>(request.Request.Category, true, out var category))
            category = CostCategory.Other;

        var costAmount = CostAmount.Create(request.Request.Amount, request.Request.Currency);

        var record = CostRecord.Create(
            _requestContext.TenantId,
            request.Request.UserId,
            request.Request.DepartmentId,
            request.Request.ProviderId,
            request.Request.ModelId,
            category,
            costAmount,
            request.Request.InputTokens,
            request.Request.OutputTokens,
            request.Request.ProcessingSeconds,
            request.Request.RequestId,
            request.Request.CorrelationId,
            request.Request.Metadata);

        await _costRecordRepository.AddAsync(record, cancellationToken);

        // Update department quota if applicable
        if (!string.IsNullOrEmpty(request.Request.DepartmentId))
        {
            var deptQuota = await _departmentQuotaRepository.GetByDepartmentIdAsync(request.Request.DepartmentId, _requestContext.TenantId, cancellationToken);
            if (deptQuota is not null && deptQuota.IsActive)
            {
                deptQuota.AddUsage(costAmount);
                await _departmentQuotaRepository.UpdateAsync(deptQuota, cancellationToken);

                if (deptQuota.UtilizationPercentage >= 80)
                {
                    var alert = Alert.QuotaThresholdExceeded(
                        _requestContext.TenantId,
                        "Department",
                        deptQuota.DepartmentId,
                        deptQuota.DepartmentName,
                        deptQuota.UtilizationPercentage,
                        80);
                    await _alertRepository.AddAsync(alert, cancellationToken);
                }
            }
        }

        // Update user quota
        var userQuota = await _userQuotaRepository.GetByUserIdAsync(request.Request.UserId, _requestContext.TenantId, cancellationToken);
        if (userQuota is not null && userQuota.IsActive)
        {
            userQuota.AddUsage(costAmount);
            userQuota.IncrementRequests();
            await _userQuotaRepository.UpdateAsync(userQuota, cancellationToken);

            if (userQuota.UtilizationPercentage >= 80)
            {
                var alert = Alert.QuotaThresholdExceeded(
                    _requestContext.TenantId,
                    "User",
                    userQuota.UserId,
                    userQuota.UserId,
                    userQuota.UtilizationPercentage,
                    80);
                await _alertRepository.AddAsync(alert, cancellationToken);
            }
        }

        // Update budget
        var budgets = await _budgetRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            null,
            BudgetStatus.Active,
            DateTimeOffset.UtcNow.Year,
            DateTimeOffset.UtcNow.Month,
            0, 10,
            cancellationToken);

        foreach (var budget in budgets)
        {
            budget.AddSpending(costAmount);
            await _budgetRepository.UpdateAsync(budget, cancellationToken);

            if (budget.IsExceeded)
            {
                var alert = Alert.BudgetThresholdExceeded(
                    _requestContext.TenantId,
                    budget.Id,
                    budget.Name,
                    budget.UtilizationPercentage,
                    100);
                await _alertRepository.AddAsync(alert, cancellationToken);
            }
        }

        return new CostRecordCommandResult(
            record.Id.Value.ToString(),
            record.UserId,
            record.Amount.Value,
            record.Amount.Currency,
            record.InputTokens,
            record.OutputTokens,
            record.RecordedAt);
    }
}

internal sealed class CreateModelCostHandler : IRequestHandler<CreateModelCostCommand, Result<ModelCostCommandResult>>
{
    private readonly IModelCostRepository _modelCostRepository;
    private readonly IRequestContext _requestContext;

    public CreateModelCostHandler(IModelCostRepository modelCostRepository, IRequestContext requestContext)
    {
        _modelCostRepository = modelCostRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<ModelCostCommandResult>> Handle(CreateModelCostCommand request, CancellationToken cancellationToken)
    {
        var existing = await _modelCostRepository.GetByProviderModelAsync(
            request.Request.ProviderId,
            request.Request.ModelId,
            _requestContext.TenantId,
            DateTimeOffset.UtcNow,
            cancellationToken);

        if (existing is not null)
            return Result.Failure<ModelCostCommandResult>(CostOptimizationErrors.ModelCostAlreadyExists);

        var modelCost = ModelCost.Create(
            _requestContext.TenantId,
            request.Request.ProviderId,
            request.Request.ModelId,
            request.Request.ModelName,
            request.Request.InputCostPerToken,
            request.Request.OutputCostPerToken,
            request.Request.CostPerRequest,
            request.Request.CostPerSecond,
            request.Request.EffectiveFrom);

        await _modelCostRepository.AddAsync(modelCost, cancellationToken);

        return new ModelCostCommandResult(
            modelCost.Id.Value.ToString(),
            modelCost.ProviderId,
            modelCost.ModelId,
            modelCost.ModelName,
            modelCost.InputCostPerToken.Value,
            modelCost.OutputCostPerToken.Value,
            modelCost.CostPerRequest.Value,
            modelCost.IsActive);
    }
}

internal sealed class UpdateModelCostHandler : IRequestHandler<UpdateModelCostCommand, Result<ModelCostCommandResult>>
{
    private readonly IModelCostRepository _modelCostRepository;
    private readonly IRequestContext _requestContext;

    public UpdateModelCostHandler(IModelCostRepository modelCostRepository, IRequestContext requestContext)
    {
        _modelCostRepository = modelCostRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<ModelCostCommandResult>> Handle(UpdateModelCostCommand request, CancellationToken cancellationToken)
    {
        var modelCost = await _modelCostRepository.GetByIdAsync(ModelCostId.Create(request.ModelCostId), _requestContext.TenantId, cancellationToken);
        if (modelCost is null)
            return Result.Failure<ModelCostCommandResult>(CostOptimizationErrors.ModelCostNotFound);

        if (request.Request.EffectiveTo.HasValue)
        {
            modelCost.Deactivate(request.Request.EffectiveTo.Value);
        }

        await _modelCostRepository.UpdateAsync(modelCost, cancellationToken);

        return new ModelCostCommandResult(
            modelCost.Id.Value.ToString(),
            modelCost.ProviderId,
            modelCost.ModelId,
            modelCost.ModelName,
            modelCost.InputCostPerToken.Value,
            modelCost.OutputCostPerToken.Value,
            modelCost.CostPerRequest.Value,
            modelCost.IsActive);
    }
}

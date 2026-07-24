using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationQueries;
using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.CostOptimization.Contracts.Responses;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationQueries;

internal sealed class GetModelCostByIdHandler : IRequestHandler<GetModelCostByIdQuery, Result<ModelCostResponse>>
{
    private readonly IModelCostRepository _modelCostRepository;
    private readonly IRequestContext _requestContext;

    public GetModelCostByIdHandler(IModelCostRepository modelCostRepository, IRequestContext requestContext)
    {
        _modelCostRepository = modelCostRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<ModelCostResponse>> Handle(GetModelCostByIdQuery request, CancellationToken cancellationToken)
    {
        var modelCost = await _modelCostRepository.GetByIdAsync(ModelCostId.Create(request.ModelCostId), _requestContext.TenantId, cancellationToken);
        if (modelCost is null)
            return Result.Failure<ModelCostResponse>(CostOptimizationErrors.ModelCostNotFound);

        return modelCost.MapToResponse();
    }
}

internal sealed class GetModelCostByProviderModelHandler : IRequestHandler<GetModelCostByProviderModelQuery, Result<ModelCostResponse>>
{
    private readonly IModelCostRepository _modelCostRepository;
    private readonly IRequestContext _requestContext;

    public GetModelCostByProviderModelHandler(IModelCostRepository modelCostRepository, IRequestContext requestContext)
    {
        _modelCostRepository = modelCostRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<ModelCostResponse>> Handle(GetModelCostByProviderModelQuery request, CancellationToken cancellationToken)
    {
        var modelCost = await _modelCostRepository.GetByProviderModelAsync(request.ProviderId, request.ModelId, _requestContext.TenantId, DateTimeOffset.UtcNow, cancellationToken);
        if (modelCost is null)
            return Result.Failure<ModelCostResponse>(CostOptimizationErrors.ModelCostNotFound);

        return modelCost.MapToResponse();
    }
}

internal sealed class GetModelCostsHandler : IRequestHandler<GetModelCostsQuery, Result<PaginatedResult<ModelCostResponse>>>
{
    private readonly IModelCostRepository _modelCostRepository;
    private readonly IRequestContext _requestContext;

    public GetModelCostsHandler(IModelCostRepository modelCostRepository, IRequestContext requestContext)
    {
        _modelCostRepository = modelCostRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<PaginatedResult<ModelCostResponse>>> Handle(GetModelCostsQuery request, CancellationToken cancellationToken)
    {
        var modelCosts = await _modelCostRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.ProviderId,
            request.Request.ModelId,
            request.Request.IsActive,
            request.Request.Skip,
            request.Request.Take,
            cancellationToken);

        var totalCount = await _modelCostRepository.CountAsync(
            _requestContext.TenantId,
            request.Request.ProviderId,
            request.Request.ModelId,
            request.Request.IsActive,
            cancellationToken);

        var items = modelCosts.Select(mc => mc.MapToResponse()).ToList();
        return new PaginatedResult<ModelCostResponse>(items, totalCount, request.Request.Skip, request.Request.Take);
    }
}

internal sealed class CalculateCostHandler : IRequestHandler<CalculateCostQuery, Result<CostCalculationResponse>>
{
    private readonly IModelCostRepository _modelCostRepository;
    private readonly IRequestContext _requestContext;

    public CalculateCostHandler(IModelCostRepository modelCostRepository, IRequestContext requestContext)
    {
        _modelCostRepository = modelCostRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<CostCalculationResponse>> Handle(CalculateCostQuery request, CancellationToken cancellationToken)
    {
        var modelCost = await _modelCostRepository.GetByProviderModelAsync(
            request.Request.ProviderId,
            request.Request.ModelId,
            _requestContext.TenantId,
            DateTimeOffset.UtcNow,
            cancellationToken);

        if (modelCost is null)
            return Result.Failure<CostCalculationResponse>(CostOptimizationErrors.ModelCostNotFound);

        var inputCost = modelCost.InputCostPerToken.Multiply(request.Request.InputTokens);
        var outputCost = modelCost.OutputCostPerToken.Multiply(request.Request.OutputTokens);
        var requestCost = modelCost.CostPerRequest;
        var computeCost = request.Request.ProcessingSeconds.HasValue
            ? modelCost.CostPerSecond.Multiply((decimal)request.Request.ProcessingSeconds.Value)
            : CostAmount.Zero(modelCost.Currency);
        var totalCost = inputCost.Add(outputCost).Add(requestCost).Add(computeCost);

        return new CostCalculationResponse(
            modelCost.ProviderId,
            modelCost.ModelId,
            request.Request.InputTokens,
            request.Request.OutputTokens,
            request.Request.ProcessingSeconds,
            totalCost.Value,
            totalCost.Currency,
            inputCost.Value,
            outputCost.Value,
            requestCost.Value,
            computeCost.Value);
    }
}

internal sealed class GetCostRecordsHandler : IRequestHandler<GetCostRecordsQuery, Result<PaginatedResult<CostRecordResponse>>>
{
    private readonly ICostRecordRepository _costRecordRepository;
    private readonly IRequestContext _requestContext;

    public GetCostRecordsHandler(ICostRecordRepository costRecordRepository, IRequestContext requestContext)
    {
        _costRecordRepository = costRecordRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<PaginatedResult<CostRecordResponse>>> Handle(GetCostRecordsQuery request, CancellationToken cancellationToken)
    {
        CostCategory? category = null;
        if (!string.IsNullOrEmpty(request.Request.Category) && Enum.TryParse<CostCategory>(request.Request.Category, true, out var c))
            category = c;

        var records = await _costRecordRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.UserId,
            request.Request.DepartmentId,
            request.Request.ProviderId,
            request.Request.ModelId,
            category,
            request.Request.From,
            request.Request.To,
            request.Request.Skip,
            request.Request.Take,
            cancellationToken);

        var totalCount = await _costRecordRepository.CountAsync(
            _requestContext.TenantId,
            request.Request.UserId,
            request.Request.DepartmentId,
            request.Request.ProviderId,
            request.Request.ModelId,
            category,
            request.Request.From,
            request.Request.To,
            cancellationToken);

        var items = records.Select(r => new CostRecordResponse(
            r.Id.Value.ToString(),
            r.UserId,
            r.DepartmentId,
            r.ProviderId,
            r.ModelId,
            r.Category.ToString(),
            r.Amount.Value,
            r.Amount.Currency,
            r.InputTokens,
            r.OutputTokens,
            r.TotalTokens,
            r.ProcessingSeconds,
            r.RequestId,
            r.CorrelationId,
            r.RecordedAt,
            r.PeriodYear,
            r.PeriodMonth,
            r.PeriodDay)).ToList();

        return new PaginatedResult<CostRecordResponse>(items, totalCount, request.Request.Skip, request.Request.Take);
    }
}

internal sealed class GetCostSummaryHandler : IRequestHandler<GetCostSummaryQuery, Result<CostSummaryResponse>>
{
    private readonly ICostRecordRepository _costRecordRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRequestContext _requestContext;

    public GetCostSummaryHandler(
        ICostRecordRepository costRecordRepository,
        IBudgetRepository budgetRepository,
        IRequestContext requestContext)
    {
        _costRecordRepository = costRecordRepository;
        _budgetRepository = budgetRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<CostSummaryResponse>> Handle(GetCostSummaryQuery request, CancellationToken cancellationToken)
    {
        var aggregation = await _costRecordRepository.GetAggregationAsync(
            _requestContext.TenantId,
            request.Request.Year,
            request.Request.Month,
            request.Request.DepartmentId,
            request.Request.UserId,
            cancellationToken);

        var daysInMonth = DateTime.DaysInMonth(request.Request.Year, request.Request.Month);
        var today = DateTimeOffset.UtcNow.Day;
        var daysPassed = request.Request.Year == DateTimeOffset.UtcNow.Year && request.Request.Month == DateTimeOffset.UtcNow.Month
            ? today
            : daysInMonth;

        var projectedEndOfMonth = daysInMonth > 0 && daysPassed > 0
            ? aggregation.TotalCost / daysPassed * daysInMonth
            : aggregation.TotalCost;

        var budgetOverage = aggregation.TotalCost - projectedEndOfMonth;

        var budgets = await _budgetRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            null,
            null,
            request.Request.Year,
            request.Request.Month,
            0, 1,
            cancellationToken);

        var budget = budgets.Count > 0 ? budgets[0] : null;
        var budgetUtilization = budget?.UtilizationPercentage ?? 0;

        var dailyBreakdown = aggregation.DailyBreakdown.Select(d => new DailyCostSummary(
            d.Day,
            d.Cost,
            d.Requests,
            d.Tokens)).ToList();

        return new CostSummaryResponse(
            request.Request.Year,
            request.Request.Month,
            request.Request.DepartmentId,
            request.Request.UserId,
            aggregation.TotalCost,
            "USD",
            aggregation.TotalRequests,
            aggregation.TotalInputTokens,
            aggregation.TotalOutputTokens,
            aggregation.CostByProvider,
            aggregation.CostByModel,
            aggregation.CostByCategory,
            dailyBreakdown,
            budgetUtilization,
            projectedEndOfMonth,
            budgetOverage);
    }
}

internal static class ModelCostMappingExtensions
{
    public static ModelCostResponse MapToResponse(this ModelCost mc) => new(
        mc.Id.Value.ToString(),
        mc.ProviderId,
        mc.ModelId,
        mc.ModelName,
        mc.InputCostPerToken.Value,
        mc.OutputCostPerToken.Value,
        mc.CostPerRequest.Value,
        mc.CostPerSecond.Value,
        mc.Currency,
        mc.IsActive,
        mc.EffectiveFrom,
        mc.EffectiveTo,
        mc.CreatedAt);
}

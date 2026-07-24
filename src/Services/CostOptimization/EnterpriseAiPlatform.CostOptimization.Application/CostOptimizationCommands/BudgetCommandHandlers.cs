using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationCommands;

internal sealed class CreateBudgetHandler : IRequestHandler<CreateBudgetCommand, Result<BudgetCommandResult>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRequestContext _requestContext;

    public CreateBudgetHandler(IBudgetRepository budgetRepository, IRequestContext requestContext)
    {
        _budgetRepository = budgetRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<BudgetCommandResult>> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BudgetPeriod>(request.Request.Period, true, out var period))
            return Result.Failure<BudgetCommandResult>(CostOptimizationErrors.InvalidBudgetPeriod);

        var budget = Budget.Create(
            _requestContext.TenantId,
            request.Request.Name,
            request.Request.Description,
            CostAmount.Create(request.Request.AllocatedAmount, request.Request.Currency),
            period,
            request.Request.PeriodYear,
            request.Request.PeriodMonth,
            request.Request.PeriodQuarter,
            request.Request.DepartmentId,
            request.Request.ProjectId);

        await _budgetRepository.AddAsync(budget, cancellationToken);

        return new BudgetCommandResult(
            budget.Id.Value.ToString(),
            budget.Name,
            budget.AllocatedAmount.Value,
            budget.SpentAmount.Value,
            budget.RemainingAmount.Value,
            budget.UtilizationPercentage,
            budget.Status.ToString());
    }
}

internal sealed class UpdateBudgetHandler : IRequestHandler<UpdateBudgetCommand, Result<BudgetCommandResult>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRequestContext _requestContext;

    public UpdateBudgetHandler(IBudgetRepository budgetRepository, IRequestContext requestContext)
    {
        _budgetRepository = budgetRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<BudgetCommandResult>> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(BudgetId.Create(request.BudgetId), _requestContext.TenantId, cancellationToken);
        if (budget is null)
            return Result.Failure<BudgetCommandResult>(CostOptimizationErrors.BudgetNotFound);

        if (request.Request.AllocatedAmount.HasValue)
            budget.UpdateAllocation(CostAmount.Create(request.Request.AllocatedAmount.Value, budget.AllocatedAmount.Currency));

        if (!string.IsNullOrEmpty(request.Request.Status) && Enum.TryParse<BudgetStatus>(request.Request.Status, true, out var status))
        {
            if (status == BudgetStatus.Suspended)
                budget.Suspend("Manually suspended");
            else if (status == BudgetStatus.Active && budget.Status == BudgetStatus.Suspended)
                budget.Resume();
        }

        await _budgetRepository.UpdateAsync(budget, cancellationToken);

        return new BudgetCommandResult(
            budget.Id.Value.ToString(),
            budget.Name,
            budget.AllocatedAmount.Value,
            budget.SpentAmount.Value,
            budget.RemainingAmount.Value,
            budget.UtilizationPercentage,
            budget.Status.ToString());
    }
}

internal sealed class SuspendBudgetHandler : IRequestHandler<SuspendBudgetCommand, Result<BudgetCommandResult>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRequestContext _requestContext;

    public SuspendBudgetHandler(IBudgetRepository budgetRepository, IRequestContext requestContext)
    {
        _budgetRepository = budgetRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<BudgetCommandResult>> Handle(SuspendBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(BudgetId.Create(request.BudgetId), _requestContext.TenantId, cancellationToken);
        if (budget is null)
            return Result.Failure<BudgetCommandResult>(CostOptimizationErrors.BudgetNotFound);

        budget.Suspend(request.Reason);
        await _budgetRepository.UpdateAsync(budget, cancellationToken);

        return new BudgetCommandResult(
            budget.Id.Value.ToString(),
            budget.Name,
            budget.AllocatedAmount.Value,
            budget.SpentAmount.Value,
            budget.RemainingAmount.Value,
            budget.UtilizationPercentage,
            budget.Status.ToString());
    }
}

internal sealed class ResumeBudgetHandler : IRequestHandler<ResumeBudgetCommand, Result<BudgetCommandResult>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRequestContext _requestContext;

    public ResumeBudgetHandler(IBudgetRepository budgetRepository, IRequestContext requestContext)
    {
        _budgetRepository = budgetRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<BudgetCommandResult>> Handle(ResumeBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(BudgetId.Create(request.BudgetId), _requestContext.TenantId, cancellationToken);
        if (budget is null)
            return Result.Failure<BudgetCommandResult>(CostOptimizationErrors.BudgetNotFound);

        budget.Resume();
        await _budgetRepository.UpdateAsync(budget, cancellationToken);

        return new BudgetCommandResult(
            budget.Id.Value.ToString(),
            budget.Name,
            budget.AllocatedAmount.Value,
            budget.SpentAmount.Value,
            budget.RemainingAmount.Value,
            budget.UtilizationPercentage,
            budget.Status.ToString());
    }
}

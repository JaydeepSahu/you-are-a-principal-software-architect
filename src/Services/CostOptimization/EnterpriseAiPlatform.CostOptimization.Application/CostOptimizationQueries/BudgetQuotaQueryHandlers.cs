using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationQueries;
using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.CostOptimization.Contracts.Responses;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationQueries;

internal sealed class GetBudgetByIdHandler : IRequestHandler<GetBudgetByIdQuery, Result<BudgetResponse>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRequestContext _requestContext;

    public GetBudgetByIdHandler(IBudgetRepository budgetRepository, IRequestContext requestContext)
    {
        _budgetRepository = budgetRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<BudgetResponse>> Handle(GetBudgetByIdQuery request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(BudgetId.Create(request.BudgetId), _requestContext.TenantId, cancellationToken);
        if (budget is null)
            return Result.Failure<BudgetResponse>(CostOptimizationErrors.BudgetNotFound);

        return new BudgetResponse(
            budget.Id.Value.ToString(),
            budget.Name,
            budget.Description,
            budget.AllocatedAmount.Value,
            budget.SpentAmount.Value,
            budget.RemainingAmount.Value,
            budget.UtilizationPercentage,
            budget.AllocatedAmount.Currency,
            budget.Period.ToString(),
            budget.PeriodYear,
            budget.PeriodMonth,
            budget.PeriodQuarter,
            budget.Status.ToString(),
            budget.DepartmentId,
            budget.ProjectId,
            budget.CreatedAt);
    }
}

internal sealed class GetBudgetsHandler : IRequestHandler<GetBudgetsQuery, Result<PaginatedResult<BudgetResponse>>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRequestContext _requestContext;

    public GetBudgetsHandler(IBudgetRepository budgetRepository, IRequestContext requestContext)
    {
        _budgetRepository = budgetRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<PaginatedResult<BudgetResponse>>> Handle(GetBudgetsQuery request, CancellationToken cancellationToken)
    {
        BudgetStatus? status = null;
        if (!string.IsNullOrEmpty(request.Request.Status) && Enum.TryParse<BudgetStatus>(request.Request.Status, true, out var s))
            status = s;

        var budgets = await _budgetRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            request.Request.ProjectId,
            status,
            request.Request.PeriodYear,
            request.Request.PeriodMonth,
            request.Request.Skip,
            request.Request.Take,
            cancellationToken);

        var totalCount = await _budgetRepository.CountAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            request.Request.ProjectId,
            status,
            request.Request.PeriodYear,
            request.Request.PeriodMonth,
            cancellationToken);

        var items = budgets.Select(b => new BudgetResponse(
            b.Id.Value.ToString(),
            b.Name,
            b.Description,
            b.AllocatedAmount.Value,
            b.SpentAmount.Value,
            b.RemainingAmount.Value,
            b.UtilizationPercentage,
            b.AllocatedAmount.Currency,
            b.Period.ToString(),
            b.PeriodYear,
            b.PeriodMonth,
            b.PeriodQuarter,
            b.Status.ToString(),
            b.DepartmentId,
            b.ProjectId,
            b.CreatedAt)).ToList();

        return new PaginatedResult<BudgetResponse>(items, totalCount, request.Request.Skip, request.Request.Take);
    }
}

internal sealed class GetDepartmentQuotaByIdHandler : IRequestHandler<GetDepartmentQuotaByIdQuery, Result<DepartmentQuotaResponse>>
{
    private readonly IDepartmentQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public GetDepartmentQuotaByIdHandler(IDepartmentQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<DepartmentQuotaResponse>> Handle(GetDepartmentQuotaByIdQuery request, CancellationToken cancellationToken)
    {
        var quota = await _quotaRepository.GetByIdAsync(DepartmentQuotaId.Create(request.QuotaId), _requestContext.TenantId, cancellationToken);
        if (quota is null)
            return Result.Failure<DepartmentQuotaResponse>(CostOptimizationErrors.QuotaNotFound);

        return quota.MapToResponse();
    }
}

internal sealed class GetDepartmentQuotaByDepartmentIdHandler : IRequestHandler<GetDepartmentQuotaByDepartmentIdQuery, Result<DepartmentQuotaResponse>>
{
    private readonly IDepartmentQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public GetDepartmentQuotaByDepartmentIdHandler(IDepartmentQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<DepartmentQuotaResponse>> Handle(GetDepartmentQuotaByDepartmentIdQuery request, CancellationToken cancellationToken)
    {
        var quota = await _quotaRepository.GetByDepartmentIdAsync(request.DepartmentId, _requestContext.TenantId, cancellationToken);
        if (quota is null)
            return Result.Failure<DepartmentQuotaResponse>(CostOptimizationErrors.QuotaNotFound);

        return quota.MapToResponse();
    }
}

internal sealed class GetDepartmentQuotasHandler : IRequestHandler<GetDepartmentQuotasQuery, Result<PaginatedResult<DepartmentQuotaResponse>>>
{
    private readonly IDepartmentQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public GetDepartmentQuotasHandler(IDepartmentQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<PaginatedResult<DepartmentQuotaResponse>>> Handle(GetDepartmentQuotasQuery request, CancellationToken cancellationToken)
    {
        var quotas = await _quotaRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            request.Request.UserId,
            request.Request.IsActive,
            request.Request.Skip,
            request.Request.Take,
            cancellationToken);

        var totalCount = await _quotaRepository.CountAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            request.Request.UserId,
            request.Request.IsActive,
            cancellationToken);

        var items = quotas.Select(q => q.MapToResponse()).ToList();
        return new PaginatedResult<DepartmentQuotaResponse>(items, totalCount, request.Request.Skip, request.Request.Take);
    }
}

internal sealed class GetUserQuotaByIdHandler : IRequestHandler<GetUserQuotaByIdQuery, Result<UserQuotaResponse>>
{
    private readonly IUserQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public GetUserQuotaByIdHandler(IUserQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<UserQuotaResponse>> Handle(GetUserQuotaByIdQuery request, CancellationToken cancellationToken)
    {
        var quota = await _quotaRepository.GetByIdAsync(UserQuotaId.Create(request.QuotaId), _requestContext.TenantId, cancellationToken);
        if (quota is null)
            return Result.Failure<UserQuotaResponse>(CostOptimizationErrors.QuotaNotFound);

        return quota.MapToResponse();
    }
}

internal sealed class GetUserQuotaByUserIdHandler : IRequestHandler<GetUserQuotaByUserIdQuery, Result<UserQuotaResponse>>
{
    private readonly IUserQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public GetUserQuotaByUserIdHandler(IUserQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<UserQuotaResponse>> Handle(GetUserQuotaByUserIdQuery request, CancellationToken cancellationToken)
    {
        var quota = await _quotaRepository.GetByUserIdAsync(request.UserId, _requestContext.TenantId, cancellationToken);
        if (quota is null)
            return Result.Failure<UserQuotaResponse>(CostOptimizationErrors.QuotaNotFound);

        return quota.MapToResponse();
    }
}

internal sealed class GetUserQuotasHandler : IRequestHandler<GetUserQuotasQuery, Result<PaginatedResult<UserQuotaResponse>>>
{
    private readonly IUserQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public GetUserQuotasHandler(IUserQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<PaginatedResult<UserQuotaResponse>>> Handle(GetUserQuotasQuery request, CancellationToken cancellationToken)
    {
        var quotas = await _quotaRepository.QueryAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            request.Request.UserId,
            request.Request.IsActive,
            request.Request.Skip,
            request.Request.Take,
            cancellationToken);

        var totalCount = await _quotaRepository.CountAsync(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            request.Request.UserId,
            request.Request.IsActive,
            cancellationToken);

        var items = quotas.Select(q => q.MapToResponse()).ToList();
        return new PaginatedResult<UserQuotaResponse>(items, totalCount, request.Request.Skip, request.Request.Take);
    }
}

internal static class QuotaMappingExtensions
{
    public static DepartmentQuotaResponse MapToResponse(this DepartmentQuota quota) => new(
        quota.Id.Value.ToString(),
        quota.DepartmentId,
        quota.DepartmentName,
        quota.MonthlyLimit.Value,
        quota.MonthlyUsage.Value,
        quota.RemainingAmount.Value,
        quota.UtilizationPercentage,
        quota.MonthlyLimit.Currency,
        quota.MaxUsers,
        quota.ActiveUsers,
        quota.IsActive,
        quota.CreatedAt,
        quota.UpdatedAt,
        quota.ExpiresAt);

    public static UserQuotaResponse MapToResponse(this UserQuota quota) => new(
        quota.Id.Value.ToString(),
        quota.UserId,
        quota.DepartmentId,
        quota.MonthlyLimit.Value,
        quota.MonthlyUsage.Value,
        quota.RemainingAmount.Value,
        quota.UtilizationPercentage,
        quota.MonthlyLimit.Currency,
        quota.MaxRequestsPerDay,
        quota.RequestsToday,
        quota.RemainingRequests,
        quota.IsActive,
        quota.CreatedAt,
        quota.UpdatedAt,
        quota.ExpiresAt);
}

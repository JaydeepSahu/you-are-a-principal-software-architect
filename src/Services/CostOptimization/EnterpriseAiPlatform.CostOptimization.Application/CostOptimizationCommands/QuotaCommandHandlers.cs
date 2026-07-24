using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Application.Abstractions;
using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using EnterpriseAiPlatform.CostOptimization.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;

namespace EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationCommands;

internal sealed class CreateDepartmentQuotaHandler : IRequestHandler<CreateDepartmentQuotaCommand, Result<DepartmentQuotaCommandResult>>
{
    private readonly IDepartmentQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public CreateDepartmentQuotaHandler(IDepartmentQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<DepartmentQuotaCommandResult>> Handle(CreateDepartmentQuotaCommand request, CancellationToken cancellationToken)
    {
        var existing = await _quotaRepository.GetByDepartmentIdAsync(request.Request.DepartmentId, _requestContext.TenantId, cancellationToken);
        if (existing is not null)
            return Result.Failure<DepartmentQuotaCommandResult>(CostOptimizationErrors.QuotaAlreadyExists);

        var quota = DepartmentQuota.Create(
            _requestContext.TenantId,
            request.Request.DepartmentId,
            request.Request.DepartmentName,
            CostAmount.Create(request.Request.MonthlyLimit, request.Request.Currency),
            request.Request.MaxUsers,
            request.Request.ExpiresAt);

        await _quotaRepository.AddAsync(quota, cancellationToken);

        return new DepartmentQuotaCommandResult(
            quota.Id.Value.ToString(),
            quota.DepartmentId,
            quota.DepartmentName,
            quota.MonthlyLimit.Value,
            quota.MonthlyUsage.Value,
            quota.RemainingAmount.Value,
            quota.UtilizationPercentage,
            quota.IsActive);
    }
}

internal sealed class UpdateDepartmentQuotaHandler : IRequestHandler<UpdateDepartmentQuotaCommand, Result<DepartmentQuotaCommandResult>>
{
    private readonly IDepartmentQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public UpdateDepartmentQuotaHandler(IDepartmentQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<DepartmentQuotaCommandResult>> Handle(UpdateDepartmentQuotaCommand request, CancellationToken cancellationToken)
    {
        var quota = await _quotaRepository.GetByIdAsync(DepartmentQuotaId.Create(request.QuotaId), _requestContext.TenantId, cancellationToken);
        if (quota is null)
            return Result.Failure<DepartmentQuotaCommandResult>(CostOptimizationErrors.QuotaNotFound);

        if (request.Request.MonthlyLimit.HasValue)
            quota.UpdateLimit(CostAmount.Create(request.Request.MonthlyLimit.Value, quota.MonthlyLimit.Currency));

        if (request.Request.MaxUsers.HasValue)
            quota.UpdateMaxUsers(request.Request.MaxUsers.Value);

        if (request.Request.IsActive.HasValue)
        {
            if (request.Request.IsActive.Value)
                quota.Reactivate();
            else
                quota.Deactivate();
        }

        await _quotaRepository.UpdateAsync(quota, cancellationToken);

        return new DepartmentQuotaCommandResult(
            quota.Id.Value.ToString(),
            quota.DepartmentId,
            quota.DepartmentName,
            quota.MonthlyLimit.Value,
            quota.MonthlyUsage.Value,
            quota.RemainingAmount.Value,
            quota.UtilizationPercentage,
            quota.IsActive);
    }
}

internal sealed class CreateUserQuotaHandler : IRequestHandler<CreateUserQuotaCommand, Result<UserQuotaCommandResult>>
{
    private readonly IUserQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public CreateUserQuotaHandler(IUserQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<UserQuotaCommandResult>> Handle(CreateUserQuotaCommand request, CancellationToken cancellationToken)
    {
        var existing = await _quotaRepository.GetByUserIdAsync(request.Request.UserId, _requestContext.TenantId, cancellationToken);
        if (existing is not null)
            return Result.Failure<UserQuotaCommandResult>(CostOptimizationErrors.QuotaAlreadyExists);

        var quota = UserQuota.Create(
            _requestContext.TenantId,
            request.Request.UserId,
            request.Request.DepartmentId,
            CostAmount.Create(request.Request.MonthlyLimit, request.Request.Currency),
            request.Request.MaxRequestsPerDay,
            request.Request.ExpiresAt);

        await _quotaRepository.AddAsync(quota, cancellationToken);

        return new UserQuotaCommandResult(
            quota.Id.Value.ToString(),
            quota.UserId,
            quota.MonthlyLimit.Value,
            quota.MonthlyUsage.Value,
            quota.RemainingAmount.Value,
            quota.UtilizationPercentage,
            quota.IsActive);
    }
}

internal sealed class UpdateUserQuotaHandler : IRequestHandler<UpdateUserQuotaCommand, Result<UserQuotaCommandResult>>
{
    private readonly IUserQuotaRepository _quotaRepository;
    private readonly IRequestContext _requestContext;

    public UpdateUserQuotaHandler(IUserQuotaRepository quotaRepository, IRequestContext requestContext)
    {
        _quotaRepository = quotaRepository;
        _requestContext = requestContext;
    }

    public async Task<Result<UserQuotaCommandResult>> Handle(UpdateUserQuotaCommand request, CancellationToken cancellationToken)
    {
        var quota = await _quotaRepository.GetByIdAsync(UserQuotaId.Create(request.QuotaId), _requestContext.TenantId, cancellationToken);
        if (quota is null)
            return Result.Failure<UserQuotaCommandResult>(CostOptimizationErrors.QuotaNotFound);

        if (request.Request.MonthlyLimit.HasValue)
            quota.UpdateLimit(CostAmount.Create(request.Request.MonthlyLimit.Value, quota.MonthlyLimit.Currency));

        if (request.Request.MaxRequestsPerDay.HasValue)
            quota.UpdateMaxRequestsPerDay(request.Request.MaxRequestsPerDay.Value);

        if (request.Request.IsActive.HasValue)
        {
            if (request.Request.IsActive.Value)
                quota.Reactivate();
            else
                quota.Deactivate();
        }

        await _quotaRepository.UpdateAsync(quota, cancellationToken);

        return new UserQuotaCommandResult(
            quota.Id.Value.ToString(),
            quota.UserId,
            quota.MonthlyLimit.Value,
            quota.MonthlyUsage.Value,
            quota.RemainingAmount.Value,
            quota.UtilizationPercentage,
            quota.IsActive);
    }
}

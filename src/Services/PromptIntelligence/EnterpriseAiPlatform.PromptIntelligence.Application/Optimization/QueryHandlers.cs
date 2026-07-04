using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Optimization;

internal sealed class GetProfileHandler(
    IProfileRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetProfileQuery, PromptOptimizationProfile>
{
    public async Task<Result<PromptOptimizationProfile>> Handle(
        GetProfileQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(
            query.ProfileId, requestContext.Current.TenantId, cancellationToken);
        return profile is not null
            ? Result.Success(profile)
            : Result.Failure<PromptOptimizationProfile>(
                ErrorDetail.Create("Profile.NotFound", "Profile not found."));
    }
}

internal sealed class GetProfilesHandler(
    IProfileRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetProfilesQuery, IReadOnlyList<PromptOptimizationProfile>>
{
    public async Task<Result<IReadOnlyList<PromptOptimizationProfile>>> Handle(
        GetProfilesQuery query,
        CancellationToken cancellationToken)
    {
        var profiles = await repository.GetByTenantAsync(
            requestContext.Current.TenantId, cancellationToken);
        return Result.Success(profiles);
    }
}

internal sealed class GetSessionHandler(
    ISessionRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetSessionQuery, PromptOptimizationSession>
{
    public async Task<Result<PromptOptimizationSession>> Handle(
        GetSessionQuery query,
        CancellationToken cancellationToken)
    {
        var session = await repository.GetByIdAsync(
            query.SessionId, requestContext.Current.TenantId, cancellationToken);
        return session is not null
            ? Result.Success(session)
            : Result.Failure<PromptOptimizationSession>(
                ErrorDetail.Create("Session.NotFound", "Session not found."));
    }
}

internal sealed class GetSessionsHandler(
    ISessionRepository repository,
    IRequestContextAccessor requestContext)
    : IQueryHandler<GetSessionsQuery, IReadOnlyList<PromptOptimizationSession>>
{
    public async Task<Result<IReadOnlyList<PromptOptimizationSession>>> Handle(
        GetSessionsQuery query,
        CancellationToken cancellationToken)
    {
        var sessions = await repository.GetByTenantAsync(
            requestContext.Current.TenantId, query.Take, query.Skip, cancellationToken);
        return Result.Success(sessions);
    }
}

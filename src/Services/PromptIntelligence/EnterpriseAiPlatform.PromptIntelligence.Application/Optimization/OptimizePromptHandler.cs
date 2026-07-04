using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Optimization;

internal sealed class OptimizePromptHandler(
    ISessionRepository sessionRepository,
    IProfileRepository profileRepository,
    IPromptOptimizer optimizer,
    IRequestContextAccessor requestContext)
    : ICommandHandler<OptimizePromptCommand, PromptOptimizationSession>
{
    public async Task<Result<PromptOptimizationSession>> Handle(
        OptimizePromptCommand command,
        CancellationToken cancellationToken)
    {
        var ctx = requestContext.Current;

        OptimizationRule rule = command.Rule;
        if (command.ProfileId is not null)
        {
            var profile = await profileRepository.GetByIdAsync(
                command.ProfileId, ctx.TenantId, cancellationToken);
            if (profile is null)
                return Result.Failure<PromptOptimizationSession>(
                    ErrorDetail.Create("Profile.NotFound", "Profile not found."));
            rule = profile.DefaultRule;
        }

        var session = new PromptOptimizationSession(
            PromptOptimizationSessionId.New(),
            ctx.TenantId,
            command.Prompt,
            rule,
            command.ProfileId);

        session.Start();

        try
        {
            var result = optimizer.Optimize(command.Prompt, rule);
            session.Complete(result);
        }
        catch (Exception ex)
        {
            session.Fail(ex.Message);
        }

        await sessionRepository.AddAsync(session, cancellationToken);
        return Result.Success(session);
    }
}

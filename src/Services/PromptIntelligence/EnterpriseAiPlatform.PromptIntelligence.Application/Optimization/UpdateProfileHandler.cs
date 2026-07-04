using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Contracts = EnterpriseAiPlatform.PromptIntelligence.Contracts;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Optimization;

internal sealed class UpdateProfileHandler(
    IProfileRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> Handle(
        UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(
            command.ProfileId, requestContext.Current.TenantId, cancellationToken);
        if (profile is null)
            return Result.Failure(ErrorDetail.Create("Profile.NotFound", "Profile not found."));

        profile.UpdateDetails(command.Request.Name, command.Request.Description);
        await repository.UpdateAsync(profile, cancellationToken);
        return Result.Success();
    }
}

internal sealed class UpdateProfileRuleHandler(
    IProfileRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<UpdateProfileRuleCommand>
{
    public async Task<Result> Handle(
        UpdateProfileRuleCommand command,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(
            command.ProfileId, requestContext.Current.TenantId, cancellationToken);
        if (profile is null)
            return Result.Failure(ErrorDetail.Create("Profile.NotFound", "Profile not found."));

        var rule = new OptimizationRule(
            (OptimizationStrategy)command.Request.DefaultRule.Strategy,
            command.Request.DefaultRule.MaxTokensTarget,
            command.Request.DefaultRule.MinTokenReductionPercent,
            command.Request.DefaultRule.PreserveSystemInstructions,
            command.Request.DefaultRule.PreserveExamples,
            command.Request.DefaultRule.PromptTemplateName,
            command.Request.DefaultRule.TemplateVariables,
            command.Request.DefaultRule.EnablePromptRewriting,
            command.Request.DefaultRule.EnablePromptCompression,
            command.Request.DefaultRule.EnableConversationSummarization,
            command.Request.DefaultRule.EnableContextTrimming,
            command.Request.DefaultRule.EnableDuplicateRemoval,
            command.Request.DefaultRule.EnableLanguageDetection);

        profile.UpdateDefaultRule(rule);
        await repository.UpdateAsync(profile, cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeactivateProfileHandler(
    IProfileRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<DeactivateProfileCommand>
{
    public async Task<Result> Handle(
        DeactivateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(
            command.ProfileId, requestContext.Current.TenantId, cancellationToken);
        if (profile is null)
            return Result.Failure(ErrorDetail.Create("Profile.NotFound", "Profile not found."));

        profile.Deactivate();
        await repository.UpdateAsync(profile, cancellationToken);
        return Result.Success();
    }
}

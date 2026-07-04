using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Contracts = EnterpriseAiPlatform.PromptIntelligence.Contracts;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Optimization;

internal sealed class CreateProfileHandler(
    IProfileRepository repository,
    IRequestContextAccessor requestContext)
    : ICommandHandler<CreateProfileCommand, PromptOptimizationProfileId>
{
    public async Task<Result<PromptOptimizationProfileId>> Handle(
        CreateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var ctx = requestContext.Current;
        var rule = MapRule(command.Request.DefaultRule);

        var profile = new PromptOptimizationProfile(
            PromptOptimizationProfileId.New(),
            ctx.TenantId,
            command.Request.Name,
            rule,
            command.Request.Description);

        await repository.AddAsync(profile, cancellationToken);
        return Result.Success(profile.Id);
    }

    private static OptimizationRule MapRule(Contracts.OptimizationRuleContract c)
        => new(
            (OptimizationStrategy)c.Strategy,
            c.MaxTokensTarget,
            c.MinTokenReductionPercent,
            c.PreserveSystemInstructions,
            c.PreserveExamples,
            c.PromptTemplateName,
            c.TemplateVariables,
            c.EnablePromptRewriting,
            c.EnablePromptCompression,
            c.EnableConversationSummarization,
            c.EnableContextTrimming,
            c.EnableDuplicateRemoval,
            c.EnableLanguageDetection);
}

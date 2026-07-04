using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.PromptIntelligence.Contracts.Requests;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Optimization;

public sealed record UpdateProfileCommand(
    PromptOptimizationProfileId ProfileId,
    UpdateProfileRequest Request) : ICommand;

public sealed record UpdateProfileRuleCommand(
    PromptOptimizationProfileId ProfileId,
    UpdateProfileRuleRequest Request) : ICommand;

public sealed record DeactivateProfileCommand(
    PromptOptimizationProfileId ProfileId) : ICommand;

public sealed record GetProfileQuery(
    PromptOptimizationProfileId ProfileId) : IQuery<PromptOptimizationProfile>;

public sealed record GetProfilesQuery : IQuery<IReadOnlyList<PromptOptimizationProfile>>;

public sealed record GetSessionQuery(
    PromptOptimizationSessionId SessionId) : IQuery<PromptOptimizationSession>;

public sealed record GetSessionsQuery(
    int Take = 50,
    int Skip = 0) : IQuery<IReadOnlyList<PromptOptimizationSession>>;

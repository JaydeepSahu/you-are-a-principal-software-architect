using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Contracts.Requests;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Optimization;

public sealed record CreateProfileCommand(
    CreateProfileRequest Request) : ICommand<PromptIntelligence.Domain.PromptOptimizationProfileId>;

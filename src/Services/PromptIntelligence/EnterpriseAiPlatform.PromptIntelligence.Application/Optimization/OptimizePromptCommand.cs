using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Domain;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Optimization;

public sealed record OptimizePromptCommand(
    string Prompt,
    OptimizationRule Rule,
    PromptOptimizationProfileId? ProfileId = null) : ICommand<PromptOptimizationSession>;

using EnterpriseAiPlatform.PromptIntelligence.Domain;

namespace EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;

public interface IPromptOptimizer
{
    OptimizationResult Optimize(string prompt, OptimizationRule rule);
}

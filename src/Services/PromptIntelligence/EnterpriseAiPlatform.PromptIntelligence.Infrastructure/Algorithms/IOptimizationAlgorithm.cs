namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure.Algorithms;

public interface IOptimizationAlgorithm
{
    string Name { get; }
    string Apply(string prompt);
}

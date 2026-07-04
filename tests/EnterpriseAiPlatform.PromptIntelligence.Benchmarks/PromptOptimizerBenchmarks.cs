using BenchmarkDotNet.Attributes;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.PromptIntelligence.Infrastructure;

namespace EnterpriseAiPlatform.PromptIntelligence.Benchmarks;

[MemoryDiagnoser]
public class PromptOptimizerBenchmarks
{
    private readonly PromptOptimizer _optimizer = new();
    private readonly OptimizationRule _balancedRule = new(OptimizationStrategy.Balanced, maxTokensTarget: 400);
    private readonly OptimizationRule _templateRule = new(
        OptimizationStrategy.TokenReduction,
        promptTemplateName: "implementation",
        templateVariables: new Dictionary<string, string>
        {
            ["context"] = "Benchmark context with service boundary and tenant isolation requirements.",
        });

    private string _shortPrompt = string.Empty;
    private string _conversationPrompt = string.Empty;
    private string _longContextPrompt = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _shortPrompt = "Could you please write a clear implementation plan in order to add token estimation and duplicate removal?";
        _conversationPrompt = string.Join(
            Environment.NewLine,
            Enumerable.Range(1, 24).Select(i => i % 2 == 0
                ? $"Assistant: Noted requirement {i}; preserve tenant isolation and audit behavior."
                : $"User: Requirement {i} asks for prompt optimization and benchmark coverage."));
        _longContextPrompt = string.Join(
            Environment.NewLine + Environment.NewLine,
            Enumerable.Range(1, 16).Select(i =>
                i == 16
                    ? "Task: Return the optimized prompt with language, summary, token counts, and changes applied."
                    : $"Background {i}: " + string.Join(' ', Enumerable.Repeat("low signal context", 60))));
    }

    [Benchmark]
    public OptimizationResult OptimizeShortPrompt()
    {
        return _optimizer.Optimize(_shortPrompt, _balancedRule);
    }

    [Benchmark]
    public OptimizationResult OptimizeConversationPrompt()
    {
        return _optimizer.Optimize(_conversationPrompt, _balancedRule);
    }

    [Benchmark]
    public OptimizationResult OptimizeLongContextPrompt()
    {
        return _optimizer.Optimize(_longContextPrompt, _balancedRule);
    }

    [Benchmark]
    public OptimizationResult OptimizeWithTemplate()
    {
        return _optimizer.Optimize(_shortPrompt, _templateRule);
    }
}

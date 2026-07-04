using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.PromptIntelligence.Infrastructure;

namespace EnterpriseAiPlatform.PromptIntelligence.UnitTests;

public sealed class PromptOptimizerTests
{
    private readonly PromptOptimizer _optimizer = new();

    [Fact]
    public void OptimizeRewritesCompressesRemovesDuplicatesAndEstimatesTokens()
    {
        var rule = new OptimizationRule(OptimizationStrategy.Balanced);
        var prompt = """
            Could you please help me with writing a response in order to explain the rollout plan.
            Return JSON.
            Return JSON.
            """;

        var result = _optimizer.Optimize(prompt, rule);

        Assert.Contains("explain the rollout plan", result.OptimizedPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Could you please", result.OptimizedPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, result.DuplicateSegmentsRemoved);
        Assert.True(result.OriginalTokenCount > 0);
        Assert.True(result.OptimizedTokenCount > 0);
        Assert.True(result.OptimizedTokenCount <= result.OriginalTokenCount);
        Assert.Equal("en", result.DetectedLanguage);
    }

    [Fact]
    public void OptimizeAppliesPromptTemplate()
    {
        var rule = new OptimizationRule(
            OptimizationStrategy.TokenReduction,
            promptTemplateName: "implementation",
            templateVariables: new Dictionary<string, string>
            {
                ["context"] = "Gateway endpoint work.",
            },
            enablePromptCompression: false,
            enableDuplicateRemoval: false);

        var result = _optimizer.Optimize("Add an optimize endpoint.", rule);

        Assert.Equal("implementation", result.PromptTemplateName);
        Assert.Contains("Task: Implement the requested change.", result.OptimizedPrompt);
        Assert.Contains("Gateway endpoint work.", result.OptimizedPrompt);
        Assert.Contains("Add an optimize endpoint.", result.OptimizedPrompt);
        Assert.Contains(result.ChangesApplied, change => change.Contains("Applied prompt template", StringComparison.Ordinal));
    }

    [Fact]
    public void OptimizeSummarizesLongConversationAndKeepsRecentTurns()
    {
        var rule = new OptimizationRule(
            OptimizationStrategy.Balanced,
            enablePromptCompression: false,
            enableDuplicateRemoval: false);
        var prompt = """
            User: We need a gateway feature.
            Assistant: The gateway should validate inputs.
            User: It also needs rate limits.
            Assistant: Rate limits should be tenant-scoped.
            User: Add prompt optimization.
            Assistant: I can add a bounded context.
            User: Return the final implementation plan.
            Assistant: Include tests and benchmarks.
            """;

        var result = _optimizer.Optimize(prompt, rule);

        Assert.NotNull(result.ConversationSummary);
        Assert.Contains("Earlier conversation summary:", result.OptimizedPrompt);
        Assert.Contains("User: Return the final implementation plan.", result.OptimizedPrompt);
        Assert.DoesNotContain(
            "User: We need a gateway feature.",
            result.OptimizedPrompt.Split("Recent conversation:", StringSplitOptions.None)[1]);
    }

    [Fact]
    public void OptimizeTrimsLowSignalContextWhenTokenTargetIsSet()
    {
        var rule = new OptimizationRule(
            OptimizationStrategy.TokenReduction,
            maxTokensTarget: 70,
            enablePromptCompression: false,
            enableDuplicateRemoval: false,
            enablePromptRewriting: false);
        var lowSignal = string.Join(' ', Enumerable.Repeat("background note", 120));
        var prompt = $"""
            Background:
            {lowSignal}

            Example:
            Keep this example concise.

            Task:
            Return an optimized prompt and include token counts.
            """;

        var result = _optimizer.Optimize(prompt, rule);

        Assert.True(result.ContextSegmentsTrimmed > 0);
        Assert.Contains("Task:", result.OptimizedPrompt);
        Assert.True(result.OptimizedTokenCount <= rule.MaxTokensTarget);
    }

    [Theory]
    [InlineData("Please create a concise summary for the quarterly planning document.", "en")]
    [InlineData("Por favor crea un resumen para el documento de planificacion.", "es")]
    [InlineData("Kripya is document ka concise summary banao.", "hi")]
    public void OptimizeDetectsLanguage(string prompt, string expectedLanguage)
    {
        var rule = new OptimizationRule(
            OptimizationStrategy.TokenReduction,
            enablePromptCompression: false,
            enableDuplicateRemoval: false);

        var result = _optimizer.Optimize(prompt, rule);

        Assert.Equal(expectedLanguage, result.DetectedLanguage);
    }
}

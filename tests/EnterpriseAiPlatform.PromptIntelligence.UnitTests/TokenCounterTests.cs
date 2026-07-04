using EnterpriseAiPlatform.PromptIntelligence.Infrastructure;

namespace EnterpriseAiPlatform.PromptIntelligence.UnitTests;

public sealed class TokenCounterTests
{
    [Fact]
    public void EstimateReturnsZeroForBlankText()
    {
        Assert.Equal(0, TokenCounter.Estimate("   "));
    }

    [Fact]
    public void EstimateUsesWordBasedHeuristic()
    {
        Assert.Equal(4, TokenCounter.Estimate("one two three"));
    }
}

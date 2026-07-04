namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure;

public static class TokenCounter
{
    /// <summary>
    /// Approximates token count using word-based heuristics.
    /// One token is approximately 0.75 words for typical English text.
    /// </summary>
    public static int Estimate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        var words = text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        return (int)Math.Ceiling(words.Length / 0.75);
    }

    public static int WordCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

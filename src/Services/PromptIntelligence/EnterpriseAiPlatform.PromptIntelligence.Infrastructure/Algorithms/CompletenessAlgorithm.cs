namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure.Algorithms;

/// <summary>
/// Completeness algorithm: ensures the prompt has sufficient context.
/// </summary>
public sealed class CompletenessAlgorithm : IOptimizationAlgorithm
{
    public string Name => "Completeness Enhancement";

    public string Apply(string prompt)
    {
        var result = prompt.Trim();

        // Add explicit output format if missing
        if (!ContainsOutputDirective(result))
        {
            result += "\n\nOutput the result in a clear, structured format.";
        }

        // Ensure the prompt ends with an action if it only describes context
        if (IsContextOnly(result) && !result.Contains('.'))
        {
            result += "\nProvide a clear and accurate response.";
        }

        return result;
    }

    private static bool ContainsOutputDirective(string text)
    {
        var directives = new[]
        {
            "output", "respond", "return", "format",
            "produce", "deliver", "give me", "provide"
        };
        return directives.Any(d => text.Contains(d, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsContextOnly(string text)
    {
        // If the prompt only describes a role/persona without asking for anything
        return text.Contains("You are a") &&
               !text.Contains("Write") &&
               !text.Contains("Generate") &&
               !text.Contains("Explain") &&
               !text.Contains("Tell") &&
               !text.Contains("Describe") &&
               !text.Contains("List");
    }
}

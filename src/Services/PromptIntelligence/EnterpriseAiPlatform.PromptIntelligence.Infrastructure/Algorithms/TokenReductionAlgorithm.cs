namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure.Algorithms;

public sealed class TokenReductionAlgorithm : IOptimizationAlgorithm
{
    public string Name => "Token Reduction";

    public string Apply(string prompt)
    {
        var result = prompt;

        // Remove filler phrases
        result = RemoveFillers(result);

        // Collapse multiple spaces/newlines
        result = NormalizeWhitespace(result);

        // Shorten common verbose patterns
        result = ShortenPhrases(result);

        return result.Trim();
    }

    private static string RemoveFillers(string text)
    {
        var fillers = new[]
        {
            "please ",
            "Please ",
            "could you please ",
            "Could you please ",
            "I would like you to ",
            "I want you to ",
            "I need you to ",
            "I need ",
            "I would like ",
            "I want ",
        };

        var result = text;
        foreach (var filler in fillers)
        {
            result = result.Replace(filler, "");
        }
        return result;
    }

    private static string NormalizeWhitespace(string text)
    {
        return System.Text.RegularExpressions.Regex
            .Replace(text.Trim(), @"\s+", " ");
    }

    private static string ShortenPhrases(string text)
    {
        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["at this point in time"] = "now",
            ["in order to"] = "to",
            ["due to the fact that"] = "because",
            ["for the purpose of"] = "to",
            ["in the event that"] = "if",
            ["with regard to"] = "regarding",
            ["for the reason that"] = "because",
            ["in spite of the fact that"] = "although",
            ["it is important to note that"] = "",
            ["please note that"] = "",
            ["kindly note"] = "",
            ["as soon as possible"] = "ASAP",
            ["at your earliest convenience"] = "soon",
        };

        var result = text;
        foreach (var (from, to) in replacements)
        {
            result = result.Replace(from, to);
        }
        return result;
    }
}

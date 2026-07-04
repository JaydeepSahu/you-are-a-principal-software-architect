namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure.Algorithms;

public sealed class CostEfficiencyAlgorithm : IOptimizationAlgorithm
{
    public string Name => "Cost Efficiency";

    public string Apply(string prompt)
    {
        var result = prompt;

        // Strip politeness filler aggressively
        result = StripPoliteness(result);

        // Remove redundant explanations
        result = RemoveRedundantExplanations(result);

        // Collapse whitespace
        result = System.Text.RegularExpressions.Regex.Replace(result.Trim(), @"\s+", " ");

        // Remove parenthetical clarifications that are obvious
        result = RemoveObviousParentheticals(result);

        // Replace verbose instruction prefixes
        result = ReplaceVerbosePrefixes(result);

        return result.Trim();
    }

    private static string StripPoliteness(string text)
    {
        var fillers = new[]
        {
            "please ", "Please ", "kindly ", "Kindly ",
            "if you could ", "If you could ",
            "would you mind ", "Would you mind ",
            "it would be great if ", "It would be great if ",
            "I would appreciate it if ", "I appreciate ",
        };

        var result = text;
        foreach (var filler in fillers)
        {
            result = result.Replace(filler, "");
        }
        return result;
    }

    private static string RemoveRedundantExplanations(string text)
    {
        // Remove phrases that restate what the instruction already means
        var redundant = new[]
        {
            " in a professional manner",
            " in a clear way",
            " in detail",
            " thoroughly",
            " as much as possible",
            " to the best of your ability",
            " with attention to detail",
            " with care",
        };

        var result = text;
        foreach (var phrase in redundant)
        {
            result = result.Replace(phrase, "");
        }
        return result;
    }

    private static string RemoveObviousParentheticals(string text)
    {
        // Remove (if applicable), (if needed), (optional) type phrases
        return System.Text.RegularExpressions.Regex.Replace(
            text,
            @"\s*\(if (?:applicable|needed|required|you want)\)",
            "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string ReplaceVerbosePrefixes(string text)
    {
        var prefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["I would like you to write"] = "Write",
            ["I want you to write"] = "Write",
            ["Can you write"] = "Write",
            ["Could you write"] = "Write",
            ["Could you please write"] = "Write",
            ["Please write"] = "Write",
            ["I need you to generate"] = "Generate",
            ["Please generate"] = "Generate",
            ["Can you generate"] = "Generate",
            ["I would like a"] = "Give me a",
            ["I need a"] = "Give me a",
        };

        var result = text;
        foreach (var (from, to) in prefixes)
        {
            if (result.StartsWith(from, StringComparison.OrdinalIgnoreCase))
            {
                result = to + result[from.Length..];
                break;
            }
        }
        return result;
    }
}

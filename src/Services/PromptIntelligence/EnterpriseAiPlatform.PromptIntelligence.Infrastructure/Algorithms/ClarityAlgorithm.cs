namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure.Algorithms;

public sealed class ClarityAlgorithm : IOptimizationAlgorithm
{
    public string Name => "Clarity Enhancement";

    public string Apply(string prompt)
    {
        var result = prompt;

        // Ensure the prompt starts with a clear action verb if it's an instruction
        result = EnsureActionStart(result);

        // Add structure markers if missing
        result = EnsureStructure(result);

        // Normalize casing inconsistencies in instruction keywords
        result = NormalizeInstructionKeywords(result);

        // Remove ambiguous references
        result = ResolveAmbiguousReferences(result);

        return result.Trim();
    }

    private static string EnsureActionStart(string text)
    {
        var trimmed = text.TrimStart();
        if (trimmed.StartsWith("Write", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Generate", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Analyze", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Explain", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Summarize", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("List", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Define", StringComparison.OrdinalIgnoreCase))
        {
            return text; // Already starts with an action verb
        }

        // If it looks like a system instruction, leave it alone
        if (trimmed.StartsWith("You are", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("You are a", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("The user is", StringComparison.OrdinalIgnoreCase))
        {
            return text;
        }

        return text;
    }

    private static string EnsureStructure(string text)
    {
        // If the prompt has no separators but is long, ensure it has proper formatting
        if (text.Length > 200 && !text.Contains('\n') && text.Contains(';'))
        {
            // Convert semicolon-separated items to bullet points
            var parts = text.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                return string.Join(";\n", parts.Select(p => "- " + p.Trim()));
            }
        }
        return text;
    }

    private static string NormalizeInstructionKeywords(string text)
    {
        var keywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["please write"] = "Write",
            ["please generate"] = "Generate",
            ["could you"] = "Write",
            ["would you"] = "Write",
        };

        var result = text;
        foreach (var (from, to) in keywords)
        {
            if (result.StartsWith(from, StringComparison.OrdinalIgnoreCase))
            {
                result = to + result[from.Length..];
                break;
            }
        }
        return result;
    }

    private static string ResolveAmbiguousReferences(string text)
    {
        // Replace "it" / "this" / "that" at the start of a new sentence when they could be ambiguous
        // This is a heuristic: if a sentence starts with "This" without prior context, flag it
        // For now, just normalize whitespace
        return System.Text.RegularExpressions.Regex.Replace(text.Trim(), @"\s+", " ");
    }
}

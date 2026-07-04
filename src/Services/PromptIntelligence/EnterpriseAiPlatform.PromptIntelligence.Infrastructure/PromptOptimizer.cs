using System.Text;
using System.Text.RegularExpressions;
using EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.PromptIntelligence.Infrastructure.Algorithms;

namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure;

public sealed class PromptOptimizer : IPromptOptimizer
{
    private readonly Dictionary<OptimizationStrategy, IOptimizationAlgorithm> _algorithms;
    private readonly Dictionary<string, string> _templates;

    public PromptOptimizer()
    {
        _algorithms = new Dictionary<OptimizationStrategy, IOptimizationAlgorithm>
        {
            [OptimizationStrategy.TokenReduction] = new TokenReductionAlgorithm(),
            [OptimizationStrategy.Clarity] = new ClarityAlgorithm(),
            [OptimizationStrategy.CostEfficiency] = new CostEfficiencyAlgorithm(),
            [OptimizationStrategy.Completeness] = new CompletenessAlgorithm(),
            [OptimizationStrategy.Balanced] = new BalancedAlgorithm(),
        };

        _templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["implementation"] = "Task: Implement the requested change.\n\nContext:\n{context}\n\nRequest:\n{prompt}\n\nReturn production-ready code, focused tests, and a brief verification summary.",
            ["bug-fix"] = "Task: Diagnose and fix the bug.\n\nObserved behavior:\n{observedBehavior}\n\nExpected behavior:\n{expectedBehavior}\n\nContext:\n{prompt}\n\nReturn the root cause, fix, tests, and verification steps.",
            ["code-review"] = "Task: Review the change for bugs, regressions, security risks, and missing tests.\n\nChange context:\n{prompt}\n\nReturn findings first, ordered by severity, with file and line references when available.",
            ["summarize"] = "Task: Summarize the material.\n\nMaterial:\n{prompt}\n\nReturn concise bullets, key decisions, risks, and follow-up actions.",
            ["test-plan"] = "Task: Create a test plan.\n\nFeature or change:\n{prompt}\n\nReturn unit, integration, security, tenant-isolation, and failure-path test cases.",
        };
    }

    public OptimizationResult Optimize(string prompt, OptimizationRule rule)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));
        }

        var originalTokens = TokenCounter.Estimate(prompt);
        var changes = new List<string>();
        var duplicateSegmentsRemoved = 0;
        var contextSegmentsTrimmed = 0;
        string? conversationSummary = null;
        var detectedLanguage = rule.EnableLanguageDetection ? DetectLanguage(prompt) : "not_requested";

        var (systemPart, userPart) = SplitSystemInstruction(prompt);
        if (rule.PreserveSystemInstructions && systemPart.Length > 0)
        {
            changes.Add("Preserved system instruction block.");
        }
        else if (!rule.PreserveSystemInstructions)
        {
            systemPart = string.Empty;
            userPart = prompt;
        }

        var optimizedUser = userPart;

        if (rule.PromptTemplateName is not null)
        {
            optimizedUser = ApplyTemplate(optimizedUser, rule, changes);
        }

        if (rule.EnableConversationSummarization)
        {
            var summarized = SummarizeConversation(optimizedUser);
            if (summarized.Summary is not null)
            {
                optimizedUser = summarized.Text;
                conversationSummary = summarized.Summary;
                changes.Add("Summarized earlier conversation turns and preserved the latest request.");
            }
        }

        if (rule.EnableDuplicateRemoval)
        {
            var deduplicated = RemoveDuplicates(optimizedUser);
            optimizedUser = deduplicated.Text;
            duplicateSegmentsRemoved = deduplicated.RemovedCount;
            if (duplicateSegmentsRemoved > 0)
            {
                changes.Add($"Removed {duplicateSegmentsRemoved} duplicate prompt segment(s).");
            }
        }

        if (rule.EnableContextTrimming && rule.MaxTokensTarget.HasValue)
        {
            var trimmed = TrimContext(optimizedUser, rule.MaxTokensTarget.Value);
            optimizedUser = trimmed.Text;
            contextSegmentsTrimmed = trimmed.TrimmedCount;
            if (contextSegmentsTrimmed > 0)
            {
                changes.Add($"Trimmed {contextSegmentsTrimmed} low-signal context segment(s).");
            }
        }

        if (rule.EnablePromptRewriting)
        {
            var rewritten = RewritePrompt(optimizedUser);
            if (rewritten != optimizedUser)
            {
                optimizedUser = rewritten;
                changes.Add("Rewrote prompt into direct task-oriented instructions.");
            }
        }

        if (rule.EnablePromptCompression)
        {
            var compressed = _algorithms[rule.Strategy].Apply(optimizedUser);
            if (compressed != optimizedUser)
            {
                optimizedUser = compressed;
                changes.Add($"Compressed prompt with {_algorithms[rule.Strategy].Name}.");
            }
            else
            {
                changes.Add($"Applied {rule.Strategy} strategy via {_algorithms[rule.Strategy].Name}.");
            }
        }

        if (rule.MaxTokensTarget.HasValue)
        {
            var beforeCap = optimizedUser;
            optimizedUser = ApplyTokenCap(optimizedUser, rule.MaxTokensTarget.Value);
            if (beforeCap != optimizedUser)
            {
                changes.Add($"Applied token cap of {rule.MaxTokensTarget} tokens.");
            }
        }

        var result = systemPart.Length > 0
            ? $"{systemPart.Trim()}\n\n{optimizedUser.Trim()}"
            : optimizedUser.Trim();

        if (rule.EnableLanguageDetection)
        {
            changes.Add($"Detected prompt language: {detectedLanguage}.");
        }

        var optimizedTokens = TokenCounter.Estimate(result);

        return new OptimizationResult(
            result,
            originalTokens,
            optimizedTokens,
            changes,
            detectedLanguage,
            conversationSummary,
            rule.PromptTemplateName,
            duplicateSegmentsRemoved,
            contextSegmentsTrimmed);
    }

    private static (string System, string User) SplitSystemInstruction(string prompt)
    {
        var match = Regex.Match(
            prompt,
            @"^(system|system prompt|instruction|instructions)[\s:]+",
            RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return (string.Empty, prompt);
        }

        var colonIndex = prompt.IndexOf(':', StringComparison.Ordinal);
        if (colonIndex < 0)
        {
            return (string.Empty, prompt);
        }

        var lineEnd = prompt.IndexOf('\n', colonIndex + 1);
        if (lineEnd < 0)
        {
            lineEnd = prompt.Length;
        }

        return (prompt[..lineEnd], prompt[lineEnd..]);
    }

    private string ApplyTemplate(string text, OptimizationRule rule, List<string> changes)
    {
        if (rule.PromptTemplateName is null ||
            !_templates.TryGetValue(rule.PromptTemplateName, out var template))
        {
            changes.Add($"Prompt template '{rule.PromptTemplateName}' was not found; used original prompt.");
            return text;
        }

        var variables = new Dictionary<string, string>(rule.TemplateVariables, StringComparer.OrdinalIgnoreCase)
        {
            ["prompt"] = text.Trim(),
        };

        if (!variables.ContainsKey("context"))
        {
            variables["context"] = "No additional context provided.";
        }

        foreach (var (key, value) in variables)
        {
            template = template.Replace("{" + key + "}", value, StringComparison.OrdinalIgnoreCase);
        }

        template = Regex.Replace(template, @"\{[A-Za-z][A-Za-z0-9_]*\}", "Not provided.");
        changes.Add($"Applied prompt template '{rule.PromptTemplateName}'.");
        return template;
    }

    private static string RewritePrompt(string text)
    {
        var result = text.Trim();
        var prefixReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["can you help me with"] = "Help with",
            ["could you please"] = "",
            ["could you"] = "",
            ["would you please"] = "",
            ["please help me"] = "Help me",
            ["i want you to"] = "",
            ["i would like you to"] = "",
            ["i need you to"] = "",
            ["make sure to"] = "Ensure",
        };

        foreach (var (from, to) in prefixReplacements)
        {
            if (result.StartsWith(from, StringComparison.OrdinalIgnoreCase))
            {
                result = to + result[from.Length..];
                break;
            }
        }

        result = Regex.Replace(result, @"\bASAP\b", "soon", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\s+", " ");
        return result.Trim();
    }

    private static (string Text, string? Summary) SummarizeConversation(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var turns = lines
            .Where(line => Regex.IsMatch(line, @"^(user|assistant|system|developer)\s*:", RegexOptions.IgnoreCase))
            .ToList();

        if (turns.Count < 6)
        {
            return (text, null);
        }

        var latestTurns = turns.TakeLast(4).ToList();
        var previousTurns = turns.Take(turns.Count - latestTurns.Count).ToList();
        var summaryItems = previousTurns
            .Select(CompactTurn)
            .Where(item => item.Length > 0)
            .Take(6)
            .ToList();

        var summary = "Earlier conversation summary: " + string.Join("; ", summaryItems) + ".";
        var optimized = new StringBuilder();
        optimized.AppendLine(summary);
        optimized.AppendLine();
        optimized.AppendLine("Recent conversation:");
        foreach (var turn in latestTurns)
        {
            optimized.AppendLine(turn);
        }

        return (optimized.ToString().Trim(), summary);
    }

    private static string CompactTurn(string turn)
    {
        var compact = Regex.Replace(turn, @"\s+", " ").Trim();
        return compact.Length <= 120 ? compact : compact[..117] + "...";
    }

    private static (string Text, int RemovedCount) RemoveDuplicates(string text)
    {
        var parts = Regex.Split(text.Trim(), @"(?<=[.!?])\s+|\r?\n")
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .ToList();

        if (parts.Count < 2)
        {
            return (text.Trim(), 0);
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var kept = new List<string>();
        var removed = 0;

        foreach (var part in parts)
        {
            var key = Regex.Replace(part, @"\s+", " ").Trim().Trim('.', '!', '?', ':', ';');
            if (!seen.Add(key))
            {
                removed++;
                continue;
            }

            kept.Add(part);
        }

        return removed == 0
            ? (text.Trim(), 0)
            : (string.Join(Environment.NewLine, kept), removed);
    }

    private static (string Text, int TrimmedCount) TrimContext(string text, int maxTokens)
    {
        if (TokenCounter.Estimate(text) <= maxTokens)
        {
            return (text, 0);
        }

        var paragraphs = Regex.Split(text.Trim(), @"\r?\n\s*\r?\n")
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .ToList();

        if (paragraphs.Count <= 2)
        {
            return (text, 0);
        }

        var scored = paragraphs
            .Select((paragraph, index) => new
            {
                Index = index,
                Score = ScoreContextParagraph(paragraph, index, paragraphs.Count),
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Index)
            .ToList();

        var selected = new List<int>();
        foreach (var item in scored)
        {
            selected.Add(item.Index);
            var candidate = string.Join(
                Environment.NewLine + Environment.NewLine,
                selected.Order().Select(index => paragraphs[index]));
            if (TokenCounter.Estimate(candidate) > maxTokens)
            {
                selected.Remove(item.Index);
            }
        }

        if (selected.Count == 0)
        {
            return (text, 0);
        }

        var selectedSet = selected.ToHashSet();
        var optimized = string.Join(
            Environment.NewLine + Environment.NewLine,
            paragraphs.Where((_, index) => selectedSet.Contains(index)));

        return (optimized, paragraphs.Count - selectedSet.Count);
    }

    private static int ScoreContextParagraph(string paragraph, int index, int total)
    {
        var score = 0;

        if (index == total - 1)
        {
            score += 12;
        }

        if (Regex.IsMatch(paragraph, @"\b(task|request|goal|requirement|return|output|error|expected|actual)\b", RegexOptions.IgnoreCase))
        {
            score += 8;
        }

        if (Regex.IsMatch(paragraph, @"\b(example|sample|for instance)\b", RegexOptions.IgnoreCase))
        {
            score += 3;
        }

        score -= Math.Min(TokenCounter.Estimate(paragraph) / 100, 6);
        return score;
    }

    private static string DetectLanguage(string text)
    {
        var normalized = " " + text.ToLowerInvariant() + " ";
        var scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = CountMatches(normalized, [" the ", " and ", " to ", " of ", " for ", " with ", " return ", " create "]),
            ["es"] = CountMatches(normalized, [" el ", " la ", " de ", " que ", " para ", " con ", " una ", " por "]),
            ["fr"] = CountMatches(normalized, [" le ", " la ", " les ", " des ", " pour ", " avec ", " une ", " que "]),
            ["de"] = CountMatches(normalized, [" der ", " die ", " das ", " und ", " mit ", " fuer ", " fur ", " nicht "]),
            ["hi"] = CountMatches(normalized, [" hai ", " aur ", " kripya ", " kya ", " ke ", " mein "]),
        };

        var devanagariChars = text.Count(ch => ch >= '\u0900' && ch <= '\u097F');
        if (devanagariChars > 0)
        {
            scores["hi"] += devanagariChars;
        }

        var best = scores.MaxBy(x => x.Value);
        return best.Value <= 0 ? "unknown" : best.Key;
    }

    private static int CountMatches(string text, IReadOnlyList<string> markers)
    {
        var count = 0;
        foreach (var marker in markers)
        {
            var index = 0;
            while ((index = text.IndexOf(marker, index, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                count++;
                index += marker.Length;
            }
        }

        return count;
    }

    private static string ApplyTokenCap(string text, int maxTokens)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= maxTokens * 0.75)
        {
            return text;
        }

        var maxWords = (int)(maxTokens * 0.75);
        var truncated = string.Join(" ", words[..maxWords]);

        var lastPeriod = truncated.LastIndexOf('.');
        var lastNewline = truncated.LastIndexOf('\n');
        var cutoff = Math.Max(lastPeriod, lastNewline);

        return cutoff > maxWords / 2
            ? truncated[..cutoff].Trim()
            : truncated + "...";
    }
}

public sealed class BalancedAlgorithm : IOptimizationAlgorithm
{
    public string Name => "Balanced Optimization";

    public string Apply(string prompt)
    {
        var result = new TokenReductionAlgorithm().Apply(prompt);
        result = new ClarityAlgorithm().Apply(result);
        return Regex.Replace(result.Trim(), @"\s+", " ");
    }
}

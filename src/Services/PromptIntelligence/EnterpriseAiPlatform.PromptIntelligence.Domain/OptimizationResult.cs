namespace EnterpriseAiPlatform.PromptIntelligence.Domain;

public sealed class OptimizationResult : SharedKernel.ValueObject
{
    public string OptimizedPrompt { get; }
    public int OriginalTokenCount { get; }
    public int OptimizedTokenCount { get; }
    public double TokenReductionPercent { get; }
    public IReadOnlyList<string> ChangesApplied { get; }
    public string DetectedLanguage { get; }
    public string? ConversationSummary { get; }
    public string? PromptTemplateName { get; }
    public int DuplicateSegmentsRemoved { get; }
    public int ContextSegmentsTrimmed { get; }
    public DateTimeOffset OptimizedAt { get; }

    public OptimizationResult(
        string optimizedPrompt,
        int originalTokenCount,
        int optimizedTokenCount,
        IReadOnlyList<string> changesApplied,
        string detectedLanguage = "unknown",
        string? conversationSummary = null,
        string? promptTemplateName = null,
        int duplicateSegmentsRemoved = 0,
        int contextSegmentsTrimmed = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(optimizedPrompt);
        ArgumentOutOfRangeException.ThrowIfNegative(originalTokenCount);
        ArgumentOutOfRangeException.ThrowIfNegative(optimizedTokenCount);
        ArgumentOutOfRangeException.ThrowIfNegative(duplicateSegmentsRemoved);
        ArgumentOutOfRangeException.ThrowIfNegative(contextSegmentsTrimmed);

        OptimizedPrompt = optimizedPrompt;
        OriginalTokenCount = originalTokenCount;
        OptimizedTokenCount = optimizedTokenCount;
        TokenReductionPercent = originalTokenCount > 0
            ? Math.Round((1 - (double)optimizedTokenCount / originalTokenCount) * 100, 2)
            : 0;
        ChangesApplied = changesApplied;
        DetectedLanguage = string.IsNullOrWhiteSpace(detectedLanguage) ? "unknown" : detectedLanguage;
        ConversationSummary = conversationSummary;
        PromptTemplateName = promptTemplateName;
        DuplicateSegmentsRemoved = duplicateSegmentsRemoved;
        ContextSegmentsTrimmed = contextSegmentsTrimmed;
        OptimizedAt = DateTimeOffset.UtcNow;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return OptimizedPrompt;
        yield return OriginalTokenCount;
        yield return OptimizedTokenCount;
        yield return TokenReductionPercent;
        yield return DetectedLanguage;
        yield return ConversationSummary;
        yield return PromptTemplateName;
        yield return DuplicateSegmentsRemoved;
        yield return ContextSegmentsTrimmed;
    }
}
